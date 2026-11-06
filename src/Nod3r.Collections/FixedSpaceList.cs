namespace Nod3r.Collections;

/// <summary>
/// A wrapper for <see cref="FixedSpaceArray{T}"/> that provides a safe list wrapper
/// around every s<see cref="Span{T}"/> of the internal array of elements.
/// This allows for more convenient access to the elements.
/// </summary>
public sealed class FixedSpaceList<T>(
    int capacity = 16,
    int metaCapacity = 4,
    params ReadOnlySpan<int> allocatedCapacities)
{
    /// <summary>
    /// The internal <see cref="FixedSpaceArray{T}"/> implementation.
    /// </summary>
    private readonly FixedSpaceArray<T> _array = new(capacity, metaCapacity, allocatedCapacities);

    /// <summary>
    /// Count of elements in every allocated span of the <see cref="FixedSpaceArray{T}"/>.
    /// </summary>
    private int[] _counts = [];
    
    
}

public ref struct FixedList<T>(Span<T> data, int count)
{
    private readonly Span<T> _data = data;
    
    public int Count { get; private set; } = count;
    
    
}
