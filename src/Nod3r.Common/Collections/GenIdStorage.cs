using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nod3r.Common.Collections;

// GenIdStorage implementation taken and modified from Space Station 14 under the MIT license

/// <summary>
/// A class that implements a generational ID array storage.
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
public sealed class GenIdStorage<T> : IEnumerable<T>
{
    private int _nextFree = int.MaxValue;
    private Slot[] _storage = [];
    private int[] _dense = [];
    
    public int Count { get; private set; }

    public ref T this[GenId id]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)id.Index >= (uint)_storage.Length)
                ThrowKeyNotFound();
            
            ref var slot = ref _storage[id.Index];
            if (slot.Generation != id.Generation)
                ThrowKeyNotFound();

            return ref slot.Value;
        }
    }
    
    public static GenIdStorage<T> FromEnumerable(IEnumerable<(GenId, T)> enumerable)
    {
        var storage = new GenIdStorage<T>();

        // Cache enumerable to array to do double enumeration.
        var cache = enumerable.ToArray();

        if (cache.Length == 0)
            return storage;

        // Figure out max size necessary and set storage size to that.
        var maxSize = cache.Max(tup => tup.Item1.Index) + 1;
        storage._storage = new Slot[maxSize];

        // Fill in slots.
        foreach (var (id, value) in cache)
        {
            Debug.Assert(id.Generation != 0, "Generation cannot be 0");

            ref var slot = ref storage._storage[id.Index];
            Debug.Assert(slot.Generation == 0, "Duplicate key index!");

            slot.Generation = id.Generation;
            slot.Value = value;
            slot.NextSlot = -1;
        }

        // Go through empty slots and build the free chain.
        var nextFree = int.MaxValue;
        for (var i = 0; i < storage._storage.Length; i++)
        {
            ref var slot = ref storage._storage[i];

            if (slot.NextSlot == -1)
                // Slot in use.
                continue;

            slot.NextSlot = nextFree;
            nextFree = i;
        }

        storage.Count = cache.Length;
        storage._nextFree = nextFree;

        return storage;
    }

    public ref T Allocate(out GenId id)
    {
        if (_nextFree == int.MaxValue)
            ReAllocate();

        var idx = _nextFree;
        ref var slot = ref _storage[idx];
        
        if (Count >= _dense.Length)
            Array.Resize(ref _dense, Math.Max(_dense.Length * 2, 4));

        _dense[Count] = idx;
        slot.DenseIndex = Count;
        
        Count += 1;
        _nextFree = slot.NextSlot;
        slot.NextSlot = -1; // Means filled

        id = new GenId(idx, slot.Generation);
        return ref slot.Value;
    }

    public void Free(GenId id)
    {
        if ((uint)id.Index >= (uint)_storage.Length)
            ThrowKeyNotFound();
        
        ref var slot = ref _storage[id.Index];
        if (slot.Generation != id.Generation || slot.NextSlot >= 0)
            ThrowKeyNotFound();

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            slot.Value = default!;

        // Swap-and-pop inside dense array
        int removedDenseIdx = slot.DenseIndex;
        int lastSlotIdx = _dense[Count - 1];
        
        _dense[removedDenseIdx] = lastSlotIdx;
        _storage[lastSlotIdx].DenseIndex = removedDenseIdx;
        
        Count -= 1;
        slot.Generation += 1;
        slot.NextSlot = _nextFree;
        _nextFree = id.Index;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ReAllocate()
    {
        int oldLength = _storage.Length;
        int newLength = Math.Max(oldLength, 2) * 2;

        ReAllocateTo(newLength);
    }

    private void ReAllocateTo(int newSize)
    {
        int oldLength = _storage.Length;
        Debug.Assert(newSize >= oldLength, "Cannot shrink GenIdStorage");

        Array.Resize(ref _storage, newSize);

        for (int i = oldLength; i < newSize; i++)
        {
            // Build linked list chain for newly allocated segment.
            ref var slot = ref _storage[i];
            slot.NextSlot = i == newSize - 1 ? _nextFree : i + 1;
            // Every slot starts at generation 1.
            slot.Generation = 1;
        }

        _storage[^1].NextSlot = _nextFree;

        _nextFree = oldLength;
    }
    
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumeratorInterface();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumeratorInterface();
    }
        
    /// <summary>
    /// Fallback enumerator that returns readonly values stored in this <see cref="GenIdStorage{T}"/>.
    /// </summary>
    private IEnumerator<T> GetEnumeratorInterface()
    {
        var slots = _storage;
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i].NextSlot < 0)
            {
                yield return slots[i].Value;
            }
        }
    }

    /// <summary>
    /// An O(Count) enumerator that returns direct references to stored objects.
    /// </summary>
    public ref struct Enumerator(GenIdStorage<T> owner)
    {
        private readonly Slot[] _slots = owner._storage;
        private readonly int[] _dense = owner._dense;
        private readonly int _count = owner.Count;
        private int _index = -1;

        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }

        public void Reset() => _index = -1;
            
        public ref T Current => ref _slots[_dense[_index]].Value;

        public void Dispose() { }
    }

    private struct Slot
    {
        /// <summary>
        /// Next link on the free list. if int.MaxValue then this is the tail.
        /// If negative, this slot is occupied.
        /// </summary>
        public int NextSlot;
        
        /// <summary>
        /// Current generation of this slot. Allows to make 
        /// </summary>
        public int Generation;
        
        /// <summary>
        /// Pointer to a position in a dense index array.
        /// Allows for faster iteration.
        /// </summary>
        public int DenseIndex;
        
        /// <summary>
        /// Data stored in this slot.
        /// </summary>
        public T Value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowKeyNotFound()
    {
        throw new KeyNotFoundException();
    }
}

public static class GenIdStorage
{
    public static GenIdStorage<T> FromEnumerable<T>(IEnumerable<(GenId, T)> enumerable)
    {
        return GenIdStorage<T>.FromEnumerable(enumerable);
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
