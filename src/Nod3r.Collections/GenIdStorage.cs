using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nod3r.Collections;

// GenIdStorage implementation taken and heavily modified from Space Station 14 under the MIT license

/// <summary>
/// An implementation of a generational ID storage.
/// </summary>
/// <remarks>
/// <para>
/// This is a very basic implementation that has the ability to automatically expand the internal array,
/// return direct references to its elements by indexing and during enumeration,
/// and also has optimizations for sparse iteration.
/// </para>
/// <para>
/// This collection is most useful for enumerating, modifying and removing large collections
/// of an undefined amount of unordered structs that get added and removed at any time.
/// </para>
/// <para>
/// When freeing a slot, instead of removing the data it is only marked as deleted,
/// allowing the next allocation to overwrite an already existing spot in memory.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of data to store.</typeparam>
public class GenIdStorage<T> : IGenStorage<T>
{
    /// <summary>
    /// Index of the next free slot.
    /// Equals to <see cref="int.MaxValue"/> when the storage is full.
    /// </summary>
    private int _nextFree;

    /// <summary>
    /// Data stored in every slot.
    /// </summary>
    internal T[] Data;
    
    /// <summary>
    /// Next link on the free list for each slot. if int.MaxValue then this is the tail.
    /// If negative, this slot is occupied.
    /// </summary>
    private int[] _nextSlots;
    
    /// <summary>
    /// Current generation for each slot.
    /// Allows for instant deletion by incrementing the generation by 1.
    /// </summary>
    private uint[] _generations;
    
    /// <summary>
    /// Dense index of every slot.
    /// </summary>
    private int[] _denseIndex;
    
    /// <summary>
    /// Pointer to a position in a dense index array.
    /// Allows for O(Count) iteration instead of O(Length).
    /// </summary>
    internal int[] Dense;

    /// <summary>
    /// Total amount of stored objects.
    /// </summary>
    public int Count { get; private set; }
    
    /// <summary>
    /// Current maximum length of the internal arrays in the storage.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Creates a new <see cref="GenIdStorage{T}"/> instance.
    /// </summary>
    /// <param name="capacity">Initial capacity of the backing array.</param>
    public GenIdStorage(int capacity = 16)
    {
        Length = capacity;
        
        Data = new T[capacity];
        _nextSlots = new int[capacity];
        _generations = new uint[capacity];
        _denseIndex = new int[capacity];
        Dense = new int[capacity];
        
        for (int i = 0; i < capacity; i++)
        {
            // Build linked list chain for newly allocated segment.
            _nextSlots[i] = i == capacity - 1 ? int.MaxValue : i + 1;
            // Every slot starts at generation 1.
            _generations[i] = 1;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"><see cref="GenId"/> for an element in the storage.</param>
    public T this[GenId id] => Get(id);

    public T Get(GenId id)
    {
        if ((uint)id.Index >= (uint)Data.Length)
            ThrowKeyNotFound();
   
        if (_generations[id.Index] != id.Generation)
            ThrowKeyNotFound();

        return Data[id.Index];
    }

    public StorageHandle<T> GetUsing(GenId id)
    {
        return new StorageHandle<T>(Get(id), id, this);
    }

    public void Set(GenId id, T value)
    {
        if ((uint)id.Index >= (uint)Data.Length)
            ThrowKeyNotFound();
   
        if (_generations[id.Index] != id.Generation)
            ThrowKeyNotFound();
        
        Data[id.Index] = value;
    }

    /// <summary>
    /// Checks whether a <see cref="GenId"/> is contained in the storage.
    /// </summary>
    /// <returns>True if <see cref="GenId"/> points to a valid element.</returns>
    public bool IsValid(GenId id)
    {
        if ((uint)id.Index >= (uint)Data.Length)
            return false;
   
        return _generations[id.Index] == id.Generation;
    }
    
    /// <summary>
    /// Adds an item to the <see cref="GenIdStorage{T}"/>
    /// </summary>
    /// <param name="item">An item to add to the storage.</param>
    /// <param name="id">A <see cref="GenId"/> handle for the item.</param>
    public void Add(T item, out GenId id)
    {
        Allocate(out id) = item;
    }
    
    /// <summary>
    /// Allocates space for an item in a <see cref="GenIdStorage{T}"/>.
    /// </summary>
    /// <param name="id">A <see cref="GenId"/> handle for the item.</param>
    /// <returns>A reference to the element in the internal array.</returns>
    public ref T Allocate(out GenId id)
    {
        if ((uint)_nextFree >= (uint)Data.Length)
            ReAllocate();

        var idx = _nextFree;
        
        Dense[Count] = idx;
        _denseIndex[idx] = Count;
        Count += 1;
        _nextFree = _nextSlots[idx];
        _nextSlots[idx] = -1; // Means filled

        id = new GenId(idx, _generations[idx]);
        return ref Data[idx];
    }

    /// <summary>
    /// Adds an item to the <see cref="GenIdStorage{T}"/>
    /// </summary>
    /// <param name="item">An item to add to the storage.</param>
    /// <returns>A <see cref="GenId"/> handle for the item.</returns>
    public GenId Add(T item)
    {
        Add(item, out var id);
        return id;
    }

    /// <summary>
    /// Marks a <see cref="GenId"/> handle as free,
    /// making the space available for new allocations.
    /// </summary>
    /// <param name="id">A <see cref="GenId"/> handle for the item.</param>
    /// <exception cref="KeyNotFoundException">
    /// <see cref="GenId"/> handle was already free or invalid.
    /// </exception>
    public void Free(GenId id)
    {
        if ((uint)id.Index >= (uint)Data.Length)
            ThrowKeyNotFound();
        
        if (_generations[id.Index] != id.Generation || _nextSlots[id.Index] >= 0)
            ThrowKeyNotFound();

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Data[id.Index] = default!;

        int removedDenseIdx = _denseIndex[id.Index];
        int lastDenseIdx = Count - 1;
    
        // Swap-and-pop inside dense array only if it's not the last element
        if (removedDenseIdx != lastDenseIdx)
        {
            int lastSlotIdx = Dense[lastDenseIdx];
            Dense[removedDenseIdx] = lastSlotIdx;
            _denseIndex[lastSlotIdx] = removedDenseIdx;
        }
        
        Count -= 1;
        _generations[id.Index] += 1;
        _nextSlots[id.Index] = _nextFree;
        _nextFree = id.Index;
    }

    /// <summary>
    /// Marks a <see cref="GenId"/> handle as free,
    /// making the space available for new allocations.
    /// </summary>
    /// <param name="id">A <see cref="GenId"/> handle for the item.</param>
    /// <returns>
    /// True if the item slot was freed successfully,
    /// False if <see cref="GenId"/> handle was already free or invalid.
    /// </returns>
    public bool TryFree(GenId id)
    {
        if (!IsValid(id))
            return false;
        
        Free(id);
        return true;
    }

    /// <summary>
    /// Tries to get an element from the storage.
    /// </summary>
    /// <param name="key">A <see cref="GenId"/> handle for the item.</param>
    /// <param name="value">The found item.</param>
    /// <returns>
    /// True if the item was found successfully,
    /// False if <see cref="GenId"/> handle was already free or invalid.
    /// </returns>
    public bool TryGetValue(GenId key, [MaybeNullWhen(false)] out T value)
    {
        value = default;
        if (!IsValid(key))
            return false;
        
        value = this[key];
        return true;
    }

    /// <summary>
    /// Ensures that the capacity of this storage is at least the specified <paramref name="capacity"/>.
    /// If the current capacity is less than <paramref name="capacity"/>,
    /// it is increased to at least the specified <paramref name="capacity"/>.</summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this storage.</returns>
    public int EnsureCapacity(int capacity)
    {
        if ((uint)capacity < (uint)Length)
            return Length;
        
        ReAllocateTo(Math.Max(Math.Max(Length, 2) * 2, capacity));
        return capacity;
    }

    private void ReAllocate()
    {
        int oldLength = Data.Length;
        int newLength = Math.Max(oldLength, 2) * 2;

        ReAllocateTo(newLength);
    }

    private void ReAllocateTo(int newSize)
    {
        int oldLength = Length;
        Debug.Assert(newSize >= oldLength, "Cannot shrink GenIdStorage");
        
        Length = newSize;
        
        Array.Resize(ref Data, newSize);
        Array.Resize(ref _nextSlots, newSize);
        Array.Resize(ref _generations, newSize);
        Array.Resize(ref _denseIndex, newSize);
        Array.Resize(ref Dense, newSize);

        for (int i = oldLength; i < newSize; i++)
        {
            // Build linked list chain for newly allocated segment.
            _nextSlots[i] = i == newSize - 1 ? _nextFree : i + 1;
            // Every slot starts at generation 1.
            _generations[i] = 1;
        }

        _nextFree = oldLength;
    }

    private static void ThrowKeyNotFound()
    {
        throw new KeyNotFoundException();
    }
}

public sealed class StorageHandle<T>(T data, GenId id, IGenStorage<T> storage) : IDisposable
{
    /// <summary>
    /// Mutable data that will be set after the wrapper is disposed.
    /// </summary>
    public T Data = data;

    /// <inheritdoc/>
    public void Dispose()
    {
        storage.Set(id, Data);
    }
}