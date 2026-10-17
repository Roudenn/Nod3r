using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.API;

/// <summary>
///     Stably identifies one voxel within some holder simulation.
/// </summary>
/// <param name="Simulation">The owning simulation registration.</param>
/// <param name="Chunk">The owning chunk.</param>
/// <param name="LocalVoxelIndex">The flat local voxel index within the chunk.</param>
/// <remarks>
///     The value contains no object references and is suitable for deterministic ordering, snapshots, and replay data.
///     It does not prove that the simulation, chunk, or voxel still exists; mutation methods validate the complete
///     address against the receiving world.
/// </remarks>
public readonly record struct VoxelRef(
    SimulationId Simulation,
    NodeIdx Type,
    NodeChunkHandle Chunk,
    ushort LocalVoxelIndex,
    byte Layer) : IComparable<VoxelRef>
{
    /// <summary>
    ///     Compares the complete stable address in simulation, chunk-coordinate, and voxel order.
    /// </summary>
    /// <param name="other">The cell reference to compare.</param>
    /// <returns>
    ///     A negative value when this address sorts first, zero when the addresses match, or a positive value when
    ///     <paramref name="other" /> sorts first.
    /// </returns>
    public int CompareTo(VoxelRef other)
    {
        int comparison = Simulation.CompareTo(other.Simulation);
        if (comparison != 0)
            return comparison;

        comparison = CompareChunkPositions(Chunk.Position, other.Chunk.Position);
        if (comparison != 0)
            return comparison;

        comparison = LocalVoxelIndex.CompareTo(other.LocalVoxelIndex);
        return comparison != 0 ? comparison : Layer.CompareTo(other.Layer);
    }

    private static int CompareChunkPositions(Int3 left, Int3 right)
    {
        int comparison = left.X.CompareTo(right.X);
        if (comparison != 0)
            return comparison;

        comparison = left.Y.CompareTo(right.Y);
        return comparison != 0 ? comparison : left.Z.CompareTo(right.Z);
    }
    
    internal VoxelHandle Handle => new(Chunk.Position, LocalVoxelIndex, Layer);

    internal VoxelRef(
        VoxelHandle handle,
        SimulationId simulation,
        NodeIdx type)
        : this(simulation, type, new NodeChunkHandle(handle.ChunkPos), handle.VoxelIndex, handle.Layer) { }
}
