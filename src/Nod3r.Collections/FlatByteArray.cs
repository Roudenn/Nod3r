using Numos.Maths;

namespace Nod3r.Collections;

/// <summary>
/// A version of Flat Array from Numos that uses a <see cref="ByteArray"/> internally.
/// </summary>
public readonly struct FlatByteArray
{
    private readonly ByteArray _data;

    private readonly Int3 _dimensions;

    /// <summary>
    ///     The total number of elements in the array.
    /// </summary>
    public int Length => _data.Length;

    /// <summary>
    ///     Maximum number of elements along each axis.
    /// </summary>
    public Int3 Dimensions => _dimensions;
    
    /// <summary>
    ///     Wraps an array as a one-dimensional flat array.
    /// </summary>
    /// <param name="data">The backing array.</param>
    /// <exception cref="ArgumentNullException"><paramref name="data" /> is <see langword="null" />.</exception>
    /// <remarks>
    ///     Why are you using this man.
    /// </remarks>
    public FlatByteArray(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        _data = new ByteArray(data);
        _dimensions = new Int3(data.Length, 1, 1);
    }

    /// <summary>
    ///     Wraps an array using the supplied dimensions for coordinate indexing.
    /// </summary>
    /// <param name="data">The backing array.</param>
    /// <param name="dimensions">The number of elements along each axis.</param>
    /// <exception cref="ArgumentNullException"><paramref name="data" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Any dimension is zero or negative.</exception>
    /// <exception cref="ArgumentException">
    ///     The backing array's length does not match the product of <paramref name="dimensions" />.
    /// </exception>
    public FlatByteArray(byte[] data, Int3 dimensions)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions.X);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions.Y);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions.Z);

        long expectedLength = (long)dimensions.X * dimensions.Y * dimensions.Z;
        if (expectedLength != data.LongLength)
        {
            throw new ArgumentException($"The array length must equal the product of its dimensions ({expectedLength}).", nameof(data));
        }

        _data = new ByteArray(data);
        _dimensions = dimensions;
    }
    
    /// <summary>
    ///     Wraps an array using the supplied dimensions for coordinate indexing.
    /// </summary>
    /// <param name="data">The backing byte array.</param>
    /// <param name="dimensions">The number of elements along each axis.</param>
    /// <exception cref="ArgumentOutOfRangeException">Any dimension is zero or negative.</exception>
    /// <exception cref="ArgumentException">
    ///     The backing array's length does not match the product of <paramref name="dimensions" />.
    /// </exception>
    public FlatByteArray(ByteArray data, Int3 dimensions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions.X);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions.Y);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions.Z);

        long expectedLength = (long)dimensions.X * dimensions.Y * dimensions.Z;
        if (expectedLength != data.Length)
        {
            throw new ArgumentException($"The array length must equal the product of its dimensions ({expectedLength}).", nameof(data));
        }

        _data = data;
        _dimensions = dimensions;
    }
    
    /// <summary>
    ///     Creates an array using the supplied dimensions for coordinate indexing.
    /// </summary>
    /// <param name="dimensions">The number of elements along each axis.</param>
    /// <exception cref="ArgumentOutOfRangeException">Any dimension is zero or negative.</exception>
    /// <exception cref="ArgumentException">
    ///     The backing array's length does not match the product of <paramref name="dimensions" />.
    /// </exception>
    public FlatByteArray(Int3 dimensions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions.X);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions.Y);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions.Z);

        long length = (long) Math.Ceiling(dimensions.X * dimensions.Y * dimensions.Z / 8.0);
        _data = new ByteArray(new byte[length]);
        _dimensions = dimensions;
    }

    /// <summary>
    ///     Returns a reference to an element by its flat array index.
    /// </summary>
    /// <param name="index">The zero-based flat array index.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index" /> is outside the backing array.</exception>
    /// <remarks>
    ///     The indexer returns by reference, so <c>flat[i] = v</c> and <c>flat[i] += v</c> write straight into the
    ///     backing array even when <c>flat</c> is a property or method result.
    /// </remarks>
    public bool this[int index]
    {
        get => _data[index];
        set => _data[index] = value;
    }

    public bool this[Int3 position]
    {
        get => _data[GetIndex(position)];
        set => _data[GetIndex(position)] = value;
    }

    /// <summary>
    ///     Converts a coordinate to its flat array index.
    /// </summary>
    /// <param name="position">The zero-based coordinate.</param>
    /// <returns>The flat index of <paramref name="position" />.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///     <paramref name="position" /> is outside <see cref="Dimensions" />.
    /// </exception>
    public int GetIndex(Int3 position)
    {
        if (!position.IsWithin(_dimensions))
            throw new IndexOutOfRangeException();

        return GetIndexUnsafe(position);
    }

    /// <summary>
    ///     Converts a coordinate to its flat array index without a bounds check.
    /// </summary>
    /// <param name="position">The zero-based coordinate.</param>
    /// <returns>The flat index of <paramref name="position" />.</returns>
    /// <remarks>
    ///     This method does not catch an out-of-range component. The result is either an index outside the array or
    ///     a silent alias of a different element (for example <c>x == Dimensions.X</c> lands on <c>(0, y + 1, z)</c>).
    /// </remarks>
    public int GetIndexUnsafe(Int3 position)
    {
        return position.X + position.Y * _dimensions.X + position.Z * _dimensions.X * _dimensions.Y;
    }


    /// <summary>
    ///     Converts a flat array index to its coordinate.
    /// </summary>
    /// <param name="index">The zero-based flat array index.</param>
    /// <returns>The coordinate stored at <paramref name="index" />.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index" /> is outside the backing array.</exception>
    public Int3 GetPosition(int index)
    {
        if ((uint)index >= (uint)Length)
            throw new IndexOutOfRangeException();

        return new Int3(
            index % _dimensions.X,
            index / _dimensions.X % _dimensions.Y,
            index / (_dimensions.X * _dimensions.Y));
    }

    /// <summary>
    ///     Clears every element in the array.
    /// </summary>
    public void Clear()
    {
        _data.AsSpan().Clear();
    }

    /// <summary>
    ///     Copies the array into the supplied destination.
    /// </summary>
    /// <param name="destination">The span to copy into. It must be at least <see cref="Length" /> long.</param>
    /// <exception cref="ArgumentException"><paramref name="destination" /> is shorter than this array.</exception>
    public void CopyTo(Span<byte> destination)
    {
        _data.AsSpan().CopyTo(destination);
    }

    /// <summary>
    ///     Copies the supplied values into the beginning of this array.
    /// </summary>
    /// <param name="source">The values to copy. Elements past its length are left unchanged.</param>
    /// <exception cref="ArgumentException"><paramref name="source" /> is longer than this array.</exception>
    public void CopyFrom(ReadOnlySpan<byte> source)
    {
        source.CopyTo(_data.AsSpan());
    }

    /// <summary>
    ///     Returns a live span over the backing storage.
    /// </summary>
    /// <remarks>
    ///     The span aliases the backing array, so writes through it are visible to every copy of this struct.
    /// </remarks>
    public Span<byte> AsSpan()
    {
        return _data.AsSpan();
    }

    /// <summary>
    ///     Returns a wrapper over the same storage using new dimensions.
    /// </summary>
    /// <param name="dimensions">The new dimensions. Their product must equal <see cref="Length" />.</param>
    /// <returns>A view sharing this instance's backing array.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Any dimension is zero or negative.</exception>
    /// <exception cref="ArgumentException">The product of <paramref name="dimensions" /> is not <see cref="Length" />.</exception>
    public FlatByteArray Reshape(Int3 dimensions)
    {
        return new FlatByteArray(_data, dimensions);
    }
}