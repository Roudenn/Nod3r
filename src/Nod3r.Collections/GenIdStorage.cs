using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nod3r.Collections;

// GenIdStorage implementation taken and heavily modified from Space Station 14 under the MIT license

/// <summary>
/// An implementation of a generational ID storage.
/// </summary>
/// <typeparam name="T">The type of data to store.</typeparam>
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
    private int[] _generations;
    
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
        Count = 0;
        Length = capacity;
        
        _data = new T[capacity];
        _nextSlots = new int[capacity];
        _generations = new int[capacity];
        _denseIndex = new int[capacity];
        _dense = new int[capacity];
        
        for (int i = 0; i < capacity; i++)
        {
            // Every slot starts at generation 1.
            _generations[i] = 1;
        }

        _nextFree = 0;
    }
    
    public ref T this[GenId id]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)id.Index >= (uint)_data.Length)
                ThrowKeyNotFound();
            
            ref var value = ref _data[id.Index];
            if (_generations[id.Index] != id.Generation)
                ThrowKeyNotFound();

            return ref value;
        }
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
    
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
        
    /// <summary>
    /// Fallback enumerator that returns readonly values stored in this <see cref="GenIdStorage{T}"/>.
    /// </summary>
    private IEnumerator<T> GetEnumeratorInterface()
    {
        for (var i = 0; i < Count; i++)
        {
            yield return _data[_dense[i]];
        }
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowKeyNotFound()
    {
        throw new KeyNotFoundException();
    }
}

public readonly record struct GenId(int Index, int Generation)
{
    public readonly static GenId Invalid = new(0, 0);
    
    public override string ToString()
    {
        return $"{Index} (G{Generation})";
    }
}
