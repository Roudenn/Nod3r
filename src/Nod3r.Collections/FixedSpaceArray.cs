using System.Collections;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Nod3r.Collections;

/// <summary>
/// Stores fixed-size spans of data inside a single contiguous array.
/// </summary>
/// <remarks>
/// <para>
/// Freed blocks are reused by allocations of the same length.
/// The backing array grows automatically when no reusable block is available.
/// </para>
/// <para>
/// This class is not thread-safe.
/// </para>
/// </remarks>
public sealed class FixedSpaceArray<T>
{
    /// <summary>
    /// The single array containing all allocated data.
    /// </summary>
    private T[] _data;

    /// <summary>
    /// Allocation metadata, indexed by GenId.Index.
    /// A zero Length indicates that the metadata slot is not currently allocated.
    /// </summary>
    private SpanInfo[] _spans;

    /// <summary>
    /// Generation counter for each allocation metadata slot.
    /// </summary>
    private uint[] _generations;
    
    /// <summary>
    /// Count of elements in every allocated span of the <see cref="FixedSpaceArray{T}"/>.
    /// </summary>
    internal int[] Counts;

    /// <summary>
    /// The next unused allocation metadata index.
    /// </summary>
    private int _nextSpanId;

    /// <summary>
    /// The end of the region of the backing array that has been used.
    /// New allocations are appended here when no reusable block is available.
    /// </summary>
    private int _usedLength;

    /// <summary>
    /// Free data blocks grouped by their exact length.
    /// The key is the block length, and the stack contains the starting
    /// offsets of reusable blocks of that length.
    /// </summary>
    // TODO PERF test this with a double nested array version to see if it works faster
    private readonly Dictionary<int, Stack<int>> _freeBlocksByLength = new();

    /// <summary>
    /// Metadata slots that can be reused for new allocations.
    /// </summary>
    private readonly Stack<int> _freeSpanIds = new();

    /// <summary>
    /// The current length of the backing data array.
    /// This increases automatically when the array needs to grow.
    /// </summary>
    public int Capacity { get; private set; }
    
    /// <summary>
    /// The current length of the metadata arrays.
    /// This increases automatically when the array needs to grow.
    /// </summary>
    public int MetaCapacity { get; private set; }

    /// <summary>
    /// The amount of the backing array that has been used at least once.
    /// This can include freed blocks.
    /// </summary>
    public int UsedLength => _usedLength;

    /// <summary>
    /// Creates a new <see cref="FixedSpaceArray{T}"/> instance.
    /// </summary>
    /// <param name="capacity">Initial capacity of the backing data array.</param>
    /// <param name="metaCapacity">Initial capacity of the metadata arrays</param>
    /// <param name="allocatedCapacities">
    /// Supported allocation lengths. Each must be between 1 and 256.
    /// When not specified, registered automatically with powers of two.
    /// </param>
    public FixedSpaceArray(
        int capacity = 16,
        int metaCapacity = 4,
        params ReadOnlySpan<int> allocatedCapacities)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(metaCapacity);

        _data = new T[capacity];
        _spans = new SpanInfo[metaCapacity];
        _generations = new uint[metaCapacity];
        Counts = new int[metaCapacity];
        Capacity = capacity;
        MetaCapacity = metaCapacity;

        if (allocatedCapacities.Length == 0)
        {
            var length = 1;
            while (length <= capacity)
            {
                AddSupportedCapacity(length);
                length *= 2;
            }
            return;
        }

        foreach (var allocatedCapacity in allocatedCapacities)
        {
            AddSupportedCapacity(allocatedCapacity);
        }
    }

    /// <summary>
    /// Registers another supported allocation length.
    /// This can also be called after the FixedSpaceArray has been initialized.
    /// </summary>
    /// <returns>
    /// True if the capacity was newly registered; false if it was already supported.
    /// </returns>
    [PublicAPI]
    public bool AddSupportedCapacity(int length)
    {
        if (_freeBlocksByLength.ContainsKey(length))
            return false;

        _freeBlocksByLength.Add(length, new Stack<int>());
        return true;
    }

    /// <summary>
    /// Gets a <see cref="SpanList{T}"/> corresponding to an allocated <see cref="GenId"/>.
    /// </summary>
    [PublicAPI]
    public SpanList<T> Get(GenId id)
    {
        if (!TryGetActiveSpan(id, out var span))
            throw new ArgumentException(
                "The ID is invalid, stale, or has already been freed.",
                nameof(id));

        return new SpanList<T>(this, id, span.Length, Counts[id.Index]);
    }

    /// <summary>
    /// Gets a <see cref="Span{T}"/> corresponding to an allocated <see cref="GenId"/>.
    /// </summary>
    /// <remarks>
    /// The provided <see cref="Span{T}"/> is only valid as long as the current
    /// <see cref="FixedSpaceArray{T}"/> doesn't allocate or free any new spans.
    /// Dangerous API.
    /// </remarks>
    [PublicAPI]
    public Span<T> GetSpan(GenId id)
    {
        if (!TryGetActiveSpan(id, out var span))
            throw new ArgumentException(
                "The ID is invalid, stale, or has already been freed.",
                nameof(id));

        return _data.AsSpan(span.Start, span.Length);
    }

    /// <summary>
    /// Allocates a span with exactly the requested supported length.
    /// </summary>
    [PublicAPI]
    public SpanList<T> Allocate(int length, out GenId genId)
    {
        AllocateInternal(length, out genId);
        return new SpanList<T>(this, genId, length);
    }

    /// <summary>
    /// Allocates a span with exactly the requested supported length.
    /// </summary>
    /// <remarks>
    /// The provided <see cref="Span{T}"/> is only valid as long as the current
    /// <see cref="FixedSpaceArray{T}"/> doesn't allocate or free any new spans.
    /// </remarks>
    [PublicAPI]
    public Span<T> AllocateSpan(int length, out GenId genId)
    {
        var info = AllocateInternal(length, out genId);
        return _data.AsSpan(info.Start, info.Length);
    }
    
    private SpanInfo AllocateInternal(int length, out GenId genId)
    {
        if (!_freeBlocksByLength.TryGetValue(length, out var freeBlocks))
        {
            throw new ArgumentException(
                $"The allocation length {length} is not supported. " +
                $"Register it using {nameof(AddSupportedCapacity)} first.",
                nameof(length));
        }

        bool isNewId = _freeSpanIds.Count == 0;
        int spanId = isNewId
            ? _nextSpanId
            : _freeSpanIds.Pop();

        if (isNewId)
        {
            EnsureMetaCapacity(checked(spanId + 1));
            _nextSpanId++;
        }
 
        int start;

        if (freeBlocks.Count > 0)
        {
            start = freeBlocks.Pop();
        }
        else
        {
            int newUsedLength = checked(_usedLength + length);
            EnsureCapacity(newUsedLength);
            start = _usedLength;
            _usedLength = newUsedLength;
        }

        genId = new GenId(spanId, _generations[spanId]);
        var info = new SpanInfo(start, length);
        _spans[spanId] = info;
        return info;
    }
    
    /// <summary>
    /// Attempts to free an allocation.
    /// Returns false if the ID is invalid, stale, or already freed.
    /// </summary>
    [PublicAPI]
    public bool Free(GenId id)
    {
        if (!TryGetActiveSpan(id, out var span))
            return false;

        FreeInternal(id.Index, span);
        return true;
    }

    /// <summary>
    /// Ensures that the backing array has at least the requested capacity.
    /// </summary>
    [PublicAPI]
    public void EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        if (capacity <= Capacity)
            return;

        int newCapacity = Math.Max(Math.Max(Capacity, 2) * 2, capacity);
        Array.Resize(ref _data, newCapacity);
        Capacity = newCapacity;
    }
    
    /// <summary>
    /// Ensures that the metadata arrays have at least the requested capacity.
    /// </summary>
    [PublicAPI]
    public void EnsureMetaCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        
        if (capacity <= MetaCapacity)
            return;

        int newCapacity = Math.Max(Math.Max(MetaCapacity, 2) * 2, capacity);
        Array.Resize(ref _spans, newCapacity);
        Array.Resize(ref _generations, newCapacity);
        Array.Resize(ref Counts, newCapacity);
        MetaCapacity = newCapacity;
    }

    private bool TryGetActiveSpan(GenId id, out SpanInfo span)
    {
        if ((uint)id.Index >= (uint)_nextSpanId
            || _generations[id.Index] != id.Generation)
        {
            span = default;
            return false;
        }

        span = _spans[id.Index];
        return span.Length > 0;
    }

    private void FreeInternal(int spanId, SpanInfo span)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Array.Clear(_data, span.Start, span.Length);
        
        _spans[spanId] = default;
        _generations[spanId]++;
        _freeBlocksByLength[span.Length].Push(span.Start);
        _freeSpanIds.Push(spanId);
    }

    /// <summary>
    /// Describes an allocation inside the backing array.
    /// </summary>
    private readonly record struct SpanInfo(int Start, int Length);
}

/// <summary>
/// <para>
/// A list wrapper for a <see cref="Span{T}"/> allocated inside a <see cref="FixedSpaceArray{T}"/>.
/// </para>
/// <para>
/// When disposed, automatically frees referenced allocation in the <see cref="FixedSpaceArray{T}"/>.
/// </para>
/// </summary>
public readonly struct SpanList<T> : IEnumerable<T>, IDisposable
{
    private readonly FixedSpaceArray<T> _owner;
    private readonly GenId _id;
    
    /// <summary>
    /// Length of the internal span of the array.
    /// </summary>
    public readonly int Length;

    /// <summary>
    /// Amount of elements stored in the <see cref="SpanList{T}"/>
    /// </summary>
    public int Count
    {
        get => _owner.Counts[_id.Index];
        private set => _owner.Counts[_id.Index] = value;
    }

    /// <summary>
    /// Gets a <see cref="Span{T}"/> of this <see cref="SpanList{T}"/>,
    /// with a length equal to its element <see cref="Count"/>.
    /// </summary>
    /// <remarks>
    /// Mutating the span without preserving the continuity of elements
    /// will break other <see cref="SpanList{T}"/> methods.
    /// Dangerous API.
    /// </remarks>
    public Span<T> Span => _owner.GetSpan(_id)[..Count];
    
    internal SpanList(FixedSpaceArray<T> owner, GenId id, int length, int count = 0)
    {
        _owner = owner;
        _id = id;
        Length = length;
        Count = count;
    }
    
    /// <summary>
    /// Tries to add an element to the <see cref="SpanList{T}"/> list.
    /// </summary>
    /// <returns>
    /// True if the element was added successfully,
    /// false if there was no room for the element in the span.
    /// </returns>
    [PublicAPI]
    public bool Add(T item)
    {
        if (Length <= Count)
            return false;
        
        _owner.GetSpan(_id)[Count] = item;
        Count++;
        return true;
    }

    /// <summary>
    /// Removes an element from a <see cref="SpanList{T}"/>.
    /// </summary>
    /// <returns>
    /// True if the item was found and removed from the span,
    /// false if the item wasn't found.
    /// </returns>
    [PublicAPI]
    public bool Remove(T item, IEqualityComparer<T>? comparer = null)
    {
        var index = IndexOf(item, comparer);
        if (index == -1)
            return false;

        var span = Span;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            span[index] = default!;
        
        span.Slice(index + 1, Count - index - 1).CopyTo(span[index..]);
        Count--;
        span[Count] = default!;
        return true;
    }

    /// <summary>
    /// Searches for the specified value and returns true if found.
    /// If not found, returns false.
    /// </summary>
    [PublicAPI]
    public bool Contains(T item, IEqualityComparer<T>? comparer = null)
    {
        return Span.Contains(item, comparer);
    }

    /// <summary>
    /// Searches for the specified value and returns the index of its first occurrence.
    /// </summary>
    [PublicAPI]
    public int IndexOf(T item, IEqualityComparer<T>? comparer = null)
    {
        return Span.IndexOf(item, comparer);
    }

    /// <summary>
    /// Sorts the elements in the entire <see cref="SpanList{T}"/> using the default comparer.
    /// </summary>
    [PublicAPI]
    public void Sort()
    {
        Span.Sort();
    }
    
    /// <summary>
    /// Sorts the elements in the entire <see cref="SpanList{T}"/> using the TComparer.
    /// </summary>
    [PublicAPI]
    public void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<T>
    {
        Span.Sort(comparer);
    }
    
    /// <summary>
    /// Reverses the sequence of the elements in the entire <see cref="SpanList{T}"/>.
    /// </summary>
    [PublicAPI]
    public void Reverse()
    {
        Span.Reverse();
    }

    /// <summary>
    /// Searches an entire sorted <see cref="SpanList{T}"/> for a specified value using the specified TComparer generic type.
    /// </summary>
    /// <returns>
    /// The zero-based index of value in the sorted span, if value is found;
    /// otherwise, a negative number that is the bitwise complement of the index
    /// of the next element that is larger than value or,
    /// if there is no larger element, the bitwise complement of Length.
    /// </returns>
    [PublicAPI]
    public int BinarySearch<TComparer>(T item, TComparer comparer) where TComparer : IComparer<T>
    {
        return Span.BinarySearch(item, comparer);
    }
    
    /// <summary>
    /// Gets an enumerator for this collection.
    /// </summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// An enumerator for <see cref="SpanList{T}"/>.
    /// </summary>
    public struct Enumerator(SpanList<T> owner) : IEnumerator<T>
    {
        private readonly SpanList<T> _owner = owner;
        private int _count = -1;
        
        /// <inheritdoc/>
        public T Current => _owner.Span[_count];
        
        object? IEnumerator.Current => Current;
        
        /// <inheritdoc/>
        public bool MoveNext()
        {
            return _count++ < _owner.Count - 1;
        }
        
        /// <inheritdoc/>
        public void Reset() { }

        /// <inheritdoc/>
        public void Dispose() { }
    }

    /// <summary>
    /// Frees the corresponding allocated span
    /// in the <see cref="FixedSpaceArray{T}"/>.
    /// </summary>
    public void Dispose()
    {
        _owner.Free(_id);
    }
}
