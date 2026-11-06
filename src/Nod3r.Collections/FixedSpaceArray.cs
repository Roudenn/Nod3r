using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Nod3r.Collections;

/// <summary>
/// Stores fixed-size spans of data inside a single contiguous array.
/// Each allocation must use a supported capacity between 1 and 256.
/// </summary>
/// <remarks>
/// <para>
/// Freed blocks are reused by allocations of the same length.
/// The backing array grows automatically when no reusable block is available.
/// </para>
/// <para>
/// This class is not thread-safe.
/// </para>
/// <para>
/// WARNING: A <see cref="Span{T}"/> returned by this class should not be
/// retained across operations that might resize the backing array. After a
/// resize, previously returned spans and memories still refer to the old array.
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
    ///
    /// The key is the block length, and the stack contains the starting
    /// offsets of reusable blocks of that length.
    /// </summary>
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

        _data = new T[capacity];
        _spans = new SpanInfo[metaCapacity];
        _generations = new uint[metaCapacity];
        Capacity = capacity;
        MetaCapacity = metaCapacity;

        if (allocatedCapacities.Length == 0)
        {
            var length = 1;
            while (length <= capacity)
            {
                AddSupportedCapacity(length - 1);
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
    /// Gets a span corresponding to an allocation ID.
    /// </summary>
    /// <remarks>
    /// The provided <see cref="Span{T}"/> is only valid as long as the current
    /// <see cref="FixedSpaceArray{T}"/> doesn't allocate or free any new spans.
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
    /// <remarks>
    /// The provided <see cref="Span{T}"/> is only valid as long as the current
    /// <see cref="FixedSpaceArray{T}"/> doesn't allocate or free any new spans.
    /// </remarks>
    [PublicAPI]
    public Span<T> Allocate(int length, out GenId genId)
    {
        if (!_freeBlocksByLength.TryGetValue(length, out var freeBlocks))
        {
            throw new ArgumentException(
                $"The allocation length {length} is not supported. " +
                $"Register it using {nameof(AddSupportedCapacity)} first.",
                nameof(length));
        }

        // Determine which metadata slot will be used.
        // Peek rather than Pop so that an allocation failure does not lose the slot.
        bool reuseId = _freeSpanIds.Count > 0;

        int spanId = reuseId
            ? _freeSpanIds.Peek()
            : _nextSpanId;

        // Ensure that the metadata arrays can hold the new allocation.
        if (!reuseId)
        {
            EnsureMetaCapacity(checked(spanId + 1));
        }

        // Find a reusable data block, or append a new block.
        bool reuseBlock = freeBlocks.Count > 0;

        int start;
        int newUsedLength = _usedLength;

        if (reuseBlock)
        {
            // Reuse a block with exactly the requested length.
            start = freeBlocks.Peek();
        }
        else
        {
            newUsedLength = checked(_usedLength + length);
            EnsureCapacity(newUsedLength);
            start = _usedLength;
        }

        // Commit the allocation.
        if (reuseId)
        {
            _freeSpanIds.Pop();
        }
        else
        {
            _nextSpanId++;
        }

        if (reuseBlock)
        {
            freeBlocks.Pop();
        }
        else
        {
            _usedLength = newUsedLength;
        }

        _spans[spanId] = new SpanInfo(start, length);
        genId = new GenId(spanId, _generations[spanId]);
        return _data.AsSpan(start, length);
    }

    /// <summary>
    /// Frees an allocation, allowing it to be reused by later allocation calls.
    /// </summary>
    /// <remarks>
    /// After an allocation is freed, a <see cref="Span{T}"/> of the freed data
    /// becomes invalid and shouldn't be accessed.
    /// </remarks>
    [PublicAPI]
    public void Free(GenId id)
    {
        if (!TryGetActiveSpan(id, out var span))
            throw new ArgumentException("The ID is invalid, stale, or has already been freed.", nameof(id));

        FreeInternal(id.Index, span);
    }

    /// <summary>
    /// Attempts to free an allocation.
    /// Returns false if the ID is invalid, stale, or already freed.
    /// </summary>
    [PublicAPI]
    public bool TryFree(GenId id)
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
