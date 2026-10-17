using Numos.Maths;

namespace Nod3r.Solver;

public static class FlatArrayHelpers
{
    /// <summary>
    ///     Converts a coordinate to its flat array index.
    /// </summary>
    public static int GetIndex(Int3 dimensions, Int3 position)
    {
        if (!position.IsWithin(dimensions))
            throw new IndexOutOfRangeException();

        return GetIndexUnsafe(dimensions, position);
    }

    /// <summary>
    ///     Converts a coordinate to its flat array index.
    /// </summary>
    public static int GetIndexUnsafe(Int3 dimensions, Int3 position)
    {
        return position.X + position.Y * dimensions.X + position.Z * dimensions.X * dimensions.Y;
    }


    /// <summary>
    ///     Converts a flat array index to its coordinate.
    /// </summary>
    public static Int3 GetPosition(Int3 dimensions, int index)
    {
        if ((uint)index >= (uint)(dimensions.X * dimensions.Y * dimensions.Z))
            throw new IndexOutOfRangeException();

        return new Int3(
            index % dimensions.X,
            index / dimensions.X % dimensions.Y,
            index / (dimensions.X * dimensions.Y));
    }
}
