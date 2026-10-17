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
public sealed class GenIdStorage<T>
{
    /// <summary>
    /// Index of the next free slot.
    /// Equals to <see cref="int.MaxValue"/> when the storage is full.
    /// </summary>
    private int _nextFree;

    /// <summary>
    /// Data stored in every slot.
    /// </summary>
    private T[] _data;
    
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
    private int[] _dense;

    /// <summary>
    /// Total amount of stored objects.
    /// </summary>
    public int Count { get; private set; }
    
    /// <summary>
    /// Current maximum length of the internal arrays in the storage.
    /// </summary>
    public int Length { get; private set; }

    public GenIdStorage(int capacity = 16)
    {
        Length = capacity;
        
        _data = new T[capacity];
        _nextSlots = new int[capacity];
        _generations = new uint[capacity];
        _denseIndex = new int[capacity];
        _dense = new int[capacity];
        
        for (int i = 0; i < capacity; i++)
        {
            // Build linked list chain for newly allocated segment.
            _nextSlots[i] = i == capacity - 1 ? int.MaxValue : i + 1;
            // Every slot starts at generation 1.
            _generations[i] = 1;
        }
    }
    
    public ref T this[GenId id]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)id.Index >= (uint)_data.Length)
                ThrowKeyNotFound();
   
            if (_generations[id.Index] != id.Generation)
                ThrowKeyNotFound();

            return ref _data[id.Index];
        }
    }

    public bool IsValid(GenId id)
    {
        if ((uint)id.Index >= (uint)_data.Length)
            return false;
   
        return _generations[id.Index] == id.Generation;
    }
    
    public void Add(T value, out GenId id)
    {
        Allocate(out id) = value;
    }
    
    public ref T Allocate(out GenId id)
    {
        if ((uint)_nextFree >= (uint)_data.Length)
            ReAllocate();

        var idx = _nextFree;
        
        _dense[Count] = idx;
        _denseIndex[idx] = Count;
        Count += 1;
        _nextFree = _nextSlots[idx];
        _nextSlots[idx] = -1; // Means filled

        id = new GenId(idx, _generations[idx]);
        return ref _data[idx];
    }

    public GenId Add(T value)
    {
        Add(value, out var id);
        return id;
    }

    public void Free(GenId id)
    {
        if ((uint)id.Index >= (uint)_data.Length)
            ThrowKeyNotFound();
        
        if (_generations[id.Index] != id.Generation || _nextSlots[id.Index] >= 0)
            ThrowKeyNotFound();

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _data[id.Index] = default!;

        int removedDenseIdx = _denseIndex[id.Index];
        int lastDenseIdx = Count - 1;
    
        // Swap-and-pop inside dense array only if it's not the last element
        if (removedDenseIdx != lastDenseIdx)
        {
            int lastSlotIdx = _dense[lastDenseIdx];
            _dense[removedDenseIdx] = lastSlotIdx;
            _denseIndex[lastSlotIdx] = removedDenseIdx;
        }
        
        Count -= 1;
        _generations[id.Index] += 1;
        _nextSlots[id.Index] = _nextFree;
        _nextFree = id.Index;
    }

    public bool TryFree(GenId id)
    {
        if (!IsValid(id))
            return false;
        
        Free(id);
        return true;
    }

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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ReAllocate()
    {
        int oldLength = _data.Length;
        int newLength = Math.Max(oldLength, 2) * 2;

        ReAllocateTo(newLength);
    }

    private void ReAllocateTo(int newSize)
    {
        int oldLength = Length;
        Debug.Assert(newSize >= oldLength, "Cannot shrink GenIdStorage");
        
        Length = newSize;
        
        Array.Resize(ref _data, newSize);
        Array.Resize(ref _nextSlots, newSize);
        Array.Resize(ref _generations, newSize);
        Array.Resize(ref _denseIndex, newSize);
        Array.Resize(ref _dense, newSize);

        for (int i = oldLength; i < newSize; i++)
        {
            // Build linked list chain for newly allocated segment.
            _nextSlots[i] = i == newSize - 1 ? _nextFree : i + 1;
            // Every slot starts at generation 1.
            _generations[i] = 1;
        }

        _nextFree = oldLength;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowKeyNotFound()
    {
        throw new KeyNotFoundException();
    }
    
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
    
    /// <summary>
    /// A custom enumerator that returns direct references to stored objects.
    /// </summary>
    public ref struct Enumerator(GenIdStorage<T> owner)
    {
        private readonly T[] _data = owner._data;
        private readonly int[] _dense = owner._dense;
        private readonly int _count = owner.Count;
        private int _index = -1;

        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }
            
        public ref T Current => ref _data[_dense[_index]];
    }
}

public readonly record struct GenId(int Index, uint Generation)
{
    public readonly static GenId Invalid = new(0, 0);
    
    public bool IsValid => Generation > 0;
    
    public override string ToString()
    {
        return $"{Index} (G{Generation})";
    }
}
