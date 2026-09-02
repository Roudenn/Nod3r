using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nod3r.Collections;

// GenIdStorage implementation taken and heavily modified from Space Station 14 under the MIT license

/// <summary>
/// A 2-dimensional implementation of a generational ID storage.
/// Instead of storing only 1 element, each slot stores an entire array of elements (column),
/// where each element sits on its own layer with an assigned generation.
/// <para>
/// The easiest way to think about this collection is that it's a <see cref="GenIdStorage{T}"/> nested twice,
/// but with access methods adapted for more control over the data and less strict checks.
/// </para>
/// </summary>
/// <typeparam name="T">The type of data to store.</typeparam>
/// <seealso cref="GenIdStorage{T}"/>
public sealed class StackIdStorage<T>
{
    /// <summary>
    /// Index of the next free slot.
    /// Equals to <see cref="int.MaxValue"/> when storage is full.
    /// </summary>
    private int _nextFree;
    
    /// <summary>
    /// Index of the next free slot in every layer.
    /// </summary>
    private int[] _nextFreeLayers;

    /// <summary>
    /// Data stored in every slot.
    /// </summary>
    private T[][] _data;
    
    /// <summary>
    /// Next link on the free list for each slot. if int.MaxValue then this is the tail.
    /// If negative, this slot is occupied.
    /// </summary>
    private int[] _nextSlots;
    
    /// <summary>
    /// Next link on the free list for each slot. if int.MaxValue then this is the tail.
    /// If negative, this slot is occupied.
    /// </summary>
    private int[][] _nextSlotLayers;
    
    /// <summary>
    /// Current generation for each slot.
    /// Allows for instant deletion by incrementing the generation by 1.
    /// </summary>
    private int[][] _generations;

    /// <summary>
    /// Current capacity of each layer.
    /// </summary>
    private int[] _layerCapacities;
    
    /// <summary>
    /// Total amount of active elements.
    /// </summary>
    public int Count { get; private set; }
    
    /// <summary>
    /// Total amount of active columns.
    /// </summary>
    public int ColumnCount { get; private set; }
    
    /// <summary>
    /// Current maximum length of the internal arrays in the storage.
    /// </summary>
    public int Length { get; private set; }
    
    /// <summary>
    /// Default array length for each layer.
    /// </summary>
    public int LayerCapacity { get; private set; }

    public StackIdStorage(int capacity = 16, byte layerCapacity = 1)
    {
        ColumnCount = 0;
        Length = capacity;
        LayerCapacity = layerCapacity;
        
        InitArray(capacity, layerCapacity, out _data);
        InitArray(capacity, layerCapacity, out _generations);
        
        _nextFreeLayers = new int[capacity];
        _nextFree = 0;
    }

    /// <summary>
    /// Helper method to initialize a nested array.
    /// </summary>
    private static void InitArray<TArray>(int capacity, int layerCapacity, out TArray[][] array)
    {
        array = new TArray[][capacity];
        for (int i = 0; i < capacity; i++)
        {
            array[i] = new TArray[layerCapacity];
        }
    }
    
    /// <summary>
    /// Helper method to extend the size of a nested array.
    /// </summary>
    private void ResizeCapacity<TArray>(ref TArray[][] array, int capacity)
    {
        array = new TArray[][capacity];
        for (int i = Length; i < capacity; i++)
        {
            array[i] = new TArray[LayerCapacity];
        }
    }
    
    /// <summary>
    /// Returns the data stored at a specific <see cref="StackGenId"/> -
    /// specific column, layer, and with a specific generation.
    /// </summary>
    public ref T this[StackGenId id]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)id.Index >= (uint)_data.Length)
                ThrowKeyNotFound();
            
            ref var value = ref _data[id.Index][id.Layer];
            if (_generations[id.Index][id.Layer] != id.Generation)
                ThrowKeyNotFound();

            return ref value;
        }
    }

    /// <summary>
    /// Allocates an entire new column in the storage and returns
    /// a reference to the element at a specified layer (first by default).
    /// </summary>
    /// <param name="id">ID of the element at the <see cref="startLayer"/>.</param>
    /// <param name="startLayer">The layer to allocate the first element on.</param>
    /// <returns></returns>
    public ref T AllocateColumn(out StackGenId id, int startLayer = 0)
    {
        if ((uint)_nextFree >= (uint)_data.Length)
            ReAllocate();

        var idx = _nextFree;
        
        ColumnCount += 1;
        _nextFree = _nextSlots[idx];
        _nextSlots[idx] = -1; // Means filled

        id = new StackGenId(idx, startLayer, _generations[idx][startLayer]);
        return ref _data[idx][startLayer];
    }
    
    /// <summary>
    /// Allocates an element on top of a specific index.
    /// </summary>
    /// <param name="index">Index of the column to allocate the element in.</param>
    /// <param name="id"><see cref="StackGenId"/> of the allocated element.</param>
    /// <returns></returns>
    public ref T Allocate(GenIdx index, out StackGenId id)
    {
        if ((uint)_nextFree >= (uint)_data.Length)
            ReAllocate();

        var idx = _nextFree;

        Count += 1;
        _nextFree = _nextSlots[idx];
        _nextSlots[idx] = -1; // Means filled

        id = new StackGenId(idx, 0, _generations[idx][0]);
        return ref _data[idx][0];
    }

    /// <summary>
    /// Frees a specific element from its column, marking it as empty
    /// and allowing it to be overwritten by another element.
    /// </summary>
    /// <param name="id">The target index to free from the storage.</param>
    public void Free(StackGenId id)
    {
        if ((uint)id.Index >= (uint)_data.Length)
            ThrowKeyNotFound();
        
        if (_generations[id.Index][id.Layer] != id.Generation || _nextSlots[id.Index] >= 0)
            ThrowKeyNotFound();

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _data[id.Index][id.Layer] = default!;
        
        ColumnCount -= 1;
        _generations[id.Index][id.Layer] += 1;
        _nextSlots[id.Index] = _nextFree;
        _nextFree = id.Index;
    }

    /// <summary>
    /// Frees the entire column of elements.
    /// </summary>
    /// <param name="id">The index of the column to free from the storage.</param>
    public void Free(GenIdx id)
    {
        if ((uint)id.Index >= (uint)_data.Length)
            ThrowKeyNotFound();
        
        if (_nextSlots[id.Index] >= 0)
            ThrowKeyNotFound();

        // TODO iterate only through filled layers
        for (int i = 0; i < _data[id.Index].Length; i++)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                _data[id.Index][i] = default!;
            
            _generations[id.Index][i] += 1;
        }
        
        ColumnCount -= 1;
        
        _nextSlots[id.Index] = _nextFree;
        _nextFree = id.Index;
    }

    /// <summary>
    /// Gets a proper generation ID of the element at a specific index and layer.
    /// This allows to convert the unsafe pair of <see cref="GenIdx"/> and integer layer
    /// to a safe <see cref="StackGenId"/> that also includes the generation.
    /// </summary>
    /// <param name="idx">Index of the column.</param>
    /// <param name="layer">Layer of the element in the column.</param>
    /// <returns>A safe reference to an element that is stored on the specified position.</returns>
    public StackGenId GetStackId(GenIdx idx, int layer)
    {
        // TODO validate that indexes are in range
        return new StackGenId(idx.Index, layer, _generations[idx.Index][layer]);
    }

    /// <summary>
    /// Returns the first free layer in a specified slot.
    /// </summary>
    public int GetFreeLayer(GenIdx idx)
    {
        // TODO
    }

    /// <summary>
    /// Ensures that the capacity of this storage is at least the specified <paramref name="capacity"/>.
    /// If the current capacity is less than <paramref name="capacity"/>,
    /// it is increased to at least the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum column capacity to ensure.</param>
    /// <returns>The new capacity of this storage.</returns>
    public int EnsureCapacity(int capacity,)
    {
        if ((uint)capacity < (uint)Length)
            return Length;
        
        ReAllocateTo(Math.Max(Math.Max(Length, 2) * 2, capacity));
        return capacity;
    }

    /// <summary>
    /// Ensures that the capacity of every column in the storage is at least the specified <paramref name="layerCapacity"/>.
    /// If the current capacity is less than <paramref name="layerCapacity"/>,
    /// it is increased to at least the specified <paramref name="layerCapacity"/>.
    /// </summary>
    /// <param name="layerCapacity">The minimum layer capacity to ensure on each column.</param>
    public void EnsureLayerCapacity(int layerCapacity)
    {
        // TODO
    }

    /// <summary>
    /// Extends this storage, reallocating it to a new place in memory.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ReAllocate()
    {
        int oldLength = _data.Length;
        int newLength = Math.Max(oldLength, 2) * 2;

        ReAllocateTo(newLength);
    }

    /// <summary>
    /// Reallocates this storage and all of its internal arrays to a bigger size.
    /// All new column arrays will have length of <see cref="LayerCapacity"/>.
    /// </summary>
    /// <param name="newSize">The new amount of columns.</param>
    private void ReAllocateTo(int newSize)
    {
        int oldLength = Length;
        Debug.Assert(newSize >= oldLength, "Cannot shrink GenIdStorage");
        
        Length = newSize;
        
        ResizeCapacity(ref _data, newSize);
        ResizeCapacity(ref _generations, newSize);
        
        Array.Resize(ref _nextSlots, newSize);
        
        Array.Resize(ref _nextFreeLayers, LayerCapacity);
        Array.Resize(ref _nextSlotLayers, LayerCapacity);

        for (int i = oldLength; i < newSize; i++)
        {
            // Build linked list chain for newly allocated segment.
            _nextSlots[i] = i == newSize - 1 ? _nextFree : i + 1;
            
            for (int j = 0; j < LayerCapacity; j++)
            {
                _nextSlotLayers[i][j] = j == newSize - 1 ? _nextFreeLayers[i] : j + 1;
                // Every new slot starts at generation 1.
                _generations[i][j] = 1;
            }
        }

        _nextFree = oldLength;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowKeyNotFound()
    {
        throw new KeyNotFoundException();
    }
}

/// <summary>
/// Index to get data from data stored in <see cref="StackIdStorage{T}"/>.
/// </summary>
/// <remarks>
/// This is safe to use for the API, since this struct
/// also contains the generation of the referenced element.
/// </remarks>
/// <param name="Index">Index of the column.</param>
/// <param name="Layer">Layer inside the column.</param>
/// <param name="Generation">Generation of the slot.</param>
public readonly record struct StackGenId(int Index, int Layer, int Generation)
{
    public readonly static StackGenId Invalid = new(0, 0, 0);

    public GenIdx Idx => new(Index);
    
    public bool IsValid() => Generation > 0;
    
    public override string ToString()
    {
        return $"{Index} (G{Generation})";
    }
}

/// <summary>
/// Represents a column in the <see cref="StackIdStorage{T}"/>.
/// </summary>
/// <remarks>
/// Doesn't contain the data about the generation,
/// therefore it's not recommended to expose this to API methods.
/// </remarks>
/// <param name="Index">Index in the internal array. Negative values are treated as invalid.</param>
public readonly record struct GenIdx(int Index)
{
    public readonly static GenIdx Invalid = new(-1);
    
    public bool IsValid() => Index >= 0;
}
