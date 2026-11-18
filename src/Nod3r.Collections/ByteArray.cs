namespace Nod3r.Collections;

/// <summary>
/// A wrapper that represents a compact array of <see cref="Boolean"/> values stored as bytes.
/// Allows for extremely compressed storage of flags.
/// </summary>
public readonly struct ByteArray
{
    private readonly byte[] _data;

    /// <summary>
    /// Whether this instance wraps a backing array.
    /// </summary>
    public bool IsInitialized => _data != null;

    /// <summary>
    /// The total number of elements in the array.
    /// </summary>
    public int Length => _data?.Length ?? 0;

    /// <summary>
    /// Wraps an already existing array of bytes.
    /// </summary>
    public ByteArray(byte[] data)
    {
        _data = data;
    }
    
    /// <summary>
    /// Creates a new byte array instance with a specified length.
    /// </summary>
    public ByteArray(int length = 16)
    {
        var capacity = Math.Max((int)Math.Ceiling(length / 8.0), 0);
        _data = new byte[capacity];
    }
    
    /// <summary>
    /// Gets or sets a boolean flag at a certain index in the byte array.
    /// </summary>
    /// <param name="index">The virtual index of the boolean value in the array.</param>
    public bool this[int index]
    {
        get
        {
            var block = (uint) _data[index / 8];
            var mask = (uint) 1 << index % 8;
            return (block & mask) > 0;
        }
        set
        {
            var blockId = index / 8;
            var mask = (uint) 1 << index % 8;
            var block = (uint) _data[blockId];
            if (value ^ (block & mask) > 0)
                _data[blockId] = (byte) (block ^ mask);
        }
    }

    /// <summary>
    /// Returns this array as a span of bytes.
    /// </summary>
    public Span<byte> AsSpan()
    {
        return _data.AsSpan();
    }
}
