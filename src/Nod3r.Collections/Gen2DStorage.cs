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
public sealed class Gen2DStorage<T>
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

    public Gen2DStorage(int capacity = 16, byte layerCapacity = 1)
    {
        ColumnCount = 0;
        Count = 0;
        Length = capacity;
        LayerCapacity = layerCapacity;
        
        InitArray(capacity, layerCapacity, out _data);
        InitArray(capacity, layerCapacity, out _generations);
        InitArray(capacity, layerCapacity, out _nextSlotLayers);
        
        _nextSlots = new int[capacity];
        _nextFreeLayers = new int[capacity];
        _layerCapacities = new int[capacity];
        
        for (int i = 0; i < capacity; i++)
        {
            _layerCapacities[i] = layerCapacity;
            _nextSlots[i] = i == capacity - 1 ? int.MaxValue : i + 1;
            _nextFreeLayers[i] = 0;
            
            for (int j = 0; j < layerCapacity; j++)
            {
                _nextSlotLayers[i][j] = j == layerCapacity - 1 ? int.MaxValue : j + 1;
                _generations[i][j] = 1;
            }
        }
        
        _nextFree = 0;
    }

    private static void InitArray<TArray>(int capacity, int layerCapacity, out TArray[][] array)
    {
        array = new TArray[capacity][];
        for (int i = 0; i < capacity; i++)
        {
            array[i] = new TArray[layerCapacity];
        }
    }
    
    private void ResizeCapacity<TArray>(ref TArray[][] array, int capacity)
    {
        Array.Resize(ref array, capacity);
        for (int i = Length; i < capacity; i++)
        {
            array[i] = new TArray[LayerCapacity];
        }
    }
    
    /// <summary>
    /// Returns the data stored at a specific <see cref="LayerId"/> -
    /// specific column, layer, and with a specific generation.
    /// </summary>
    public T this[LayerId id]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)id.Index >= (uint)_data.Length)
                ThrowKeyNotFound();
            
            if ((uint)id.Layer >= (uint)_layerCapacities[id.Index])
                ThrowKeyNotFound();

            if (_generations[id.Index][id.Layer] != id.Generation || _nextSlotLayers[id.Index][id.Layer] >= 0)
                ThrowKeyNotFound();

            return _data[id.Index][id.Layer];
        }
    }

    /// <summary>
    /// Allocates an entire new column in the storage and adds the first element.
    /// </summary>
    /// <param name="value">The value to add into the new slot at <see cref="startLayer"/>.</param>
    /// <param name="id">ID of the element at the <see cref="startLayer"/>.</param>
    /// <param name="startLayer">The layer to allocate the first element on.</param>
    /// <returns>A reference to the element at a specified layer (first by default).</returns>
    public void AddColumn(T value, out LayerId id, int startLayer = 0)
    {
        if ((uint)_nextFree >= (uint)_data.Length)
            ReAllocate();

        var idx = _nextFree;
        _nextFree = _nextSlots[idx];
        _nextSlots[idx] = -1; 
        
        ColumnCount += 1;
        Count += 1;
        
        if (startLayer >= _layerCapacities[idx])
        {
            EnsureColumnLayerCapacity(idx, startLayer + 1);
        }

        int prev = -1;
        int curr = _nextFreeLayers[idx];
        while (curr != int.MaxValue && curr != startLayer)
        {
            prev = curr;
            curr = _nextSlotLayers[idx][curr];
        }

        if (curr == startLayer)
        {
            if (prev == -1) 
                _nextFreeLayers[idx] = _nextSlotLayers[idx][curr];
            else 
                _nextSlotLayers[idx][prev] = _nextSlotLayers[idx][curr];
        }
        
        _nextSlotLayers[idx][startLayer] = -1;

        id = new LayerId(idx, startLayer, _generations[idx][startLayer]);
        _data[idx][startLayer] = value;
    }

    /// <summary>
    /// Adds an element on top of a specific index.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="index">Index of the column to allocate the element in.</param>
    /// <param name="id"><see cref="LayerId"/> of the allocated element.</param>
    /// <returns>The reference to the allocated element.</returns>
    public void Add(T value, ColumnHandle index, out LayerId id)
    {
        var idx = index.Index;
        if ((uint)idx >= (uint)_data.Length || _nextSlots[idx] >= 0)
            ThrowKeyNotFound();

        int layer = _nextFreeLayers[idx];
        if (layer == int.MaxValue)
        {
            int newCap = Math.Max(_layerCapacities[idx], 2) * 2;
            EnsureColumnLayerCapacity(idx, newCap);
            layer = _nextFreeLayers[idx];
        }

        _nextFreeLayers[idx] = _nextSlotLayers[idx][layer];
        _nextSlotLayers[idx][layer] = -1;

        Count += 1;

        id = new LayerId(idx, layer, _generations[idx][layer]);
        _data[idx][layer] = value;
    }

    /// <summary>
    /// Frees a specific element from its column, marking it as empty
    /// and allowing it to be overwritten by another element.
    /// </summary>
    /// <param name="id">The target index to free from the storage.</param>
    public void Free(LayerId id)
    {
        if ((uint)id.Index >= (uint)_data.Length)
            ThrowKeyNotFound();
        
        if ((uint)id.Layer >= (uint)_layerCapacities[id.Index])
            ThrowKeyNotFound();
            
        if (_generations[id.Index][id.Layer] != id.Generation || _nextSlots[id.Index] >= 0 || _nextSlotLayers[id.Index][id.Layer] >= 0)
            ThrowKeyNotFound();

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _data[id.Index][id.Layer] = default!;
        
        Count -= 1;
        _generations[id.Index][id.Layer] += 1;
        
        _nextSlotLayers[id.Index][id.Layer] = _nextFreeLayers[id.Index];
        _nextFreeLayers[id.Index] = id.Layer;
    }

    /// <summary>
    /// Frees the entire column of elements.
    /// </summary>
    /// <param name="id">The index of the column to free from the storage.</param>
    // TODO implement column generations to make column removal O(1) instead of O(n)
    public void Free(ColumnHandle id)
    {
        if ((uint)id.Index >= (uint)_data.Length)
            ThrowKeyNotFound();
        
        if (_nextSlots[id.Index] >= 0)
            ThrowKeyNotFound();

        int freedElements = 0;
        int cap = _layerCapacities[id.Index];

        for (int i = 0; i < cap; i++)
        {
            if (_nextSlotLayers[id.Index][i] < 0)
            {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    _data[id.Index][i] = default!;
                
                _generations[id.Index][i] += 1;
                freedElements++;
            }
            
            _nextSlotLayers[id.Index][i] = i == cap - 1 ? int.MaxValue : i + 1;
        }
        
        Count -= freedElements;
        ColumnCount -= 1;
        _nextFreeLayers[id.Index] = 0;
        
        _nextSlots[id.Index] = _nextFree;
        _nextFree = id.Index;
    }

    /// <summary>
    /// Gets a proper generation ID of the element at a specific index and layer.
    /// This allows to convert the unsafe pair of <see cref="ColumnHandle"/> and integer layer
    /// to a safe <see cref="LayerId"/> that also includes the generation.
    /// </summary>
    /// <param name="idx">Index of the column.</param>
    /// <param name="layer">Layer of the element in the column.</param>
    /// <returns>A safe reference to an element that is stored on the specified position.</returns>
    public LayerId GetLayerId(ColumnHandle idx, int layer)
    {
        if ((uint)idx.Index >= (uint)_data.Length || _nextSlots[idx.Index] >= 0)
            ThrowKeyNotFound();
            
        if ((uint)layer >= (uint)_layerCapacities[idx.Index])
            ThrowKeyNotFound();

        return new LayerId(idx.Index, layer, _generations[idx.Index][layer]);
    }

    /// <summary>
    /// Returns the first free layer in a specified slot.
    /// </summary>
    public int GetFreeLayer(ColumnHandle idx)
    {
        if ((uint)idx.Index >= (uint)_data.Length || _nextSlots[idx.Index] >= 0)
            ThrowKeyNotFound();

        return _nextFreeLayers[idx.Index];
    }
    
    /// <summary>
    /// Ensures that the capacity of this storage is at least the specified <paramref name="capacity"/>.
    /// If the current capacity is less than <paramref name="capacity"/>,
    /// it is increased to at least the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum column capacity to ensure.</param>
    /// <returns>The new capacity of this storage.</returns>
    public int EnsureCapacity(int capacity)
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
        if (layerCapacity <= LayerCapacity) return;

        for (int i = 0; i < Length; i++)
        {
            EnsureColumnLayerCapacity(i, layerCapacity);
        }
        LayerCapacity = layerCapacity;
    }

    private void EnsureColumnLayerCapacity(int idx, int newCap)
    {
        int oldCap = _layerCapacities[idx];
        if (newCap <= oldCap) return;

        Array.Resize(ref _data[idx], newCap);
        Array.Resize(ref _generations[idx], newCap);
        Array.Resize(ref _nextSlotLayers[idx], newCap);

        for (int j = oldCap; j < newCap; j++)
        {
            _nextSlotLayers[idx][j] = j == newCap - 1 ? _nextFreeLayers[idx] : j + 1;
            _generations[idx][j] = 1;
        }
        
        _nextFreeLayers[idx] = oldCap;
        _layerCapacities[idx] = newCap;
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
        if (newSize == oldLength) return;
        
        Length = newSize;
        
        ResizeCapacity(ref _data, newSize);
        ResizeCapacity(ref _generations, newSize);
        ResizeCapacity(ref _nextSlotLayers, newSize);
        
        Array.Resize(ref _nextSlots, newSize);
        Array.Resize(ref _nextFreeLayers, newSize);
        Array.Resize(ref _layerCapacities, newSize);

        for (int i = oldLength; i < newSize; i++)
        {
            _layerCapacities[i] = LayerCapacity;
            _nextSlots[i] = (i == newSize - 1) ? _nextFree : i + 1;
            _nextFreeLayers[i] = 0;
            
            for (int j = 0; j < LayerCapacity; j++)
            {
                _nextSlotLayers[i][j] = j == LayerCapacity - 1 ? int.MaxValue : j + 1;
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
/// A generational identifier that uniquely references a specific element in the storage.
/// </summary>
public readonly record struct LayerId(int Index, int Layer, int Generation)
{
    public static readonly LayerId Invalid = new(-1, -1, 0);

    public ColumnHandle ColumnHandle => new(Index);
    
    public bool IsValid => Generation > 0 && Index >= 0 && Layer >= 0;

    public override string ToString() => $"[Col: {Index}, Layer: {Layer}, Gen: {Generation}]";
}

/// <summary>
/// Represents a reference to a specific column without layer or generation validation.
/// </summary>
public readonly record struct ColumnHandle(int Index)
{
    public static readonly ColumnHandle Invalid = new(-1);
    
    public bool IsValid => Index >= 0;
}
