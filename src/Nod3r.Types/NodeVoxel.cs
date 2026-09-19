using Numos.Maths;

namespace Nod3r.Types;

/// <summary>
/// Represents a position of a specific node in the simulation.
/// </summary>
/// <param name="Handle">The target <see cref="NodeVoxelHandle"/>.</param>
/// <param name="TypeId">Index type of node.</param>
public record struct NodeVoxel(NodeVoxelHandle Handle, NodeIdx TypeId);

/// <summary>
/// Represents a position of a specific node in the simulation.
/// </summary>
/// <param name="Chunk">The target chunk handle.</param>
/// <param name="Pos">Position in space inside a chunk.</param>
/// <param name="Layer">Layer of the node.</param>
public record struct NodeVoxelHandle(NodeChunkHandle Chunk, Int3 Pos, int Layer);
