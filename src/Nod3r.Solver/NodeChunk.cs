using Nod3r.Collections;
using Numos.Collections;
using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Stores references to nodes in the local storage of a node kernel as 3D chunks.
/// </summary>
internal sealed class NodeChunk
{
    /// <summary>
    /// Array that stores <see cref="GenId"/>s that reference a node
    /// of a certain type in the <see cref="NodeStorage{T}"/>.
    /// </summary>
    public FlatArray<ColumnHandle> Handles;
    
    public NodeChunk(
        Int3 position,
        Int3 dimensions)
    {
        int voxelCount = GetValidatedVoxelCount(dimensions.X, dimensions.Y, dimensions.Z);
        GridPosition = position;
        Dimensions = dimensions;
        VoxelCount = voxelCount;
        EnsureInitialized(ref Handles, Dimensions);
        Handles.Fill(ColumnHandle.Invalid);
    }

    /// <summary>
    ///     The position of this chunk in the chunk grid.
    /// </summary>
    public Int3 GridPosition;
    
    /// <summary>
    ///     Total number of voxels in this chunk, equal to <c>Width * Height * Depth</c>.
    /// </summary>
    public int VoxelCount;
    
    /// <summary>
    ///     The number of voxels along each axis.
    /// </summary>
    public Int3 Dimensions;
    
    /// <summary>
    ///     A process-local index assigned when this chunk is registered.
    /// </summary>
    /// <remarks>
    ///     Lets hot per-tick solver lookups (e.g. boundary flow's cross-chunk injection buffer) use array
    ///     indexing instead of a dictionary keyed on <see cref="GridPosition" />. Unique only among currently
    ///     registered chunks, and not part of the simulation's observable state.
    ///     Do not use it for anything but indexing a solver-owned lookup table.
    ///     Ids are recycled after a chunk is unregistered, so a DenseId-keyed lookup table must track which
    ///     chunk currently owns each slot and validate that before trusting stale contents.
    /// </remarks>
    public int DenseId;
    
    /// <summary>
    ///     Converts local voxel coordinates to an index into the chunk's flat arrays.
    /// </summary>
    /// <param name="x">The local x coordinate, from zero through <see cref="Width" /> minus one.</param>
    /// <param name="y">The local y coordinate, from zero through <see cref="Height" /> minus one.</param>
    /// <param name="z">The local z coordinate, from zero through <see cref="Depth" /> minus one.</param>
    /// <returns>The flat voxel index.</returns>
    public ushort GetIndex(int x, int y, int z)
    {
        return GetIndex(new Int3(x, y, z));
    }

    /// <inheritdoc cref="GetIndex(int, int, int)" />
    public ushort GetIndex(Int3 vec)
    {
        return (ushort)FlatArrayHelpers.GetIndex(vec, Dimensions);
    }

    /// <inheritdoc cref="GetIndex(int, int, int)" />
    public ushort GetIndexUnsafe(Int3 vec)
    {
        return (ushort)FlatArrayHelpers.GetIndexUnsafe(vec, Dimensions);
    }

    /// <summary>
    ///     Converts a flat voxel index to local x, y, and z coordinates.
    /// </summary>
    /// <param name="index">The flat voxel index.</param>
    /// <returns>The local coordinates as an <c>(x, y, z)</c> tuple.</returns>
    public (int x, int y, int z) GetXyz(ushort index)
    {
        var position = GetXyzInt3(index);
        return (position.X, position.Y, position.Z);
    }

    /// <summary>
    ///     Converts a flat voxel index to local coordinates as an <see cref="Int3" />.
    /// </summary>
    /// <param name="index">The flat voxel index.</param>
    /// <returns>The local voxel coordinates.</returns>
    public Int3 GetXyzInt3(ushort index)
    {
        return FlatArrayHelpers.GetPosition(Dimensions, index);
    }
    
    /// <summary>
    ///     Ensures that a chunk's <see cref="FlatArray{T}"/> has specified dimensions.
    /// </summary>
    /// <param name="array">The target flat array.</param>
    /// <param name="dimensions">Dimensions to ensure on the flat array.</param>
    /// <typeparam name="T">Type parameter of the flat array.</typeparam>
    private void EnsureInitialized<T>(ref FlatArray<T> array, Int3 dimensions)
    {
        if (!array.IsInitialized || array.Length != VoxelCount)
            array = new FlatArray<T>(new T[VoxelCount], dimensions);
        else if (array.Dimensions != dimensions)
            array = array.Reshape(dimensions);
    }

    /// <summary>
    ///     Validates the specified dimensions and returns the total voxel count of the chunk.
    /// </summary>
    /// <param name="width">Width of the chunk.</param>
    /// <param name="height">Height of the chunk.</param>
    /// <param name="depth">Depth of the chunk.</param>
    /// <returns>Total voxel count of the chunk.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     One of the dimensions is not in the supported size range.
    /// </exception>
    private static int GetValidatedVoxelCount(int width, int height, int depth)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(depth);
        if (width > ChunkConstants.MaximumVoxelCount ||
            height > ChunkConstants.MaximumVoxelCount ||
            depth > ChunkConstants.MaximumVoxelCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                $"No chunk dimension may exceed {ChunkConstants.MaximumVoxelCount}.");
        }

        long voxelCount = (long)width * height * depth;
        if (voxelCount > ChunkConstants.MaximumVoxelCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                $"Chunk dimensions contain {voxelCount} voxels, but at most " +
                $"{ChunkConstants.MaximumVoxelCount} are supported.");
        }

        return (int)voxelCount;
    }
}
