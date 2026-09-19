using Numos.Maths;

namespace Nod3r.Types;

/// <summary>
/// Represents a specific node with a type in the <see cref="INodeSolver"/>.
/// </summary>
/// <param name="Handle">The target position inside a node kernel.</param>
/// <param name="TypeId">Index type of node.</param>
public record struct NodeVoxel(NodeVoxelHandle Handle, NodeIdx TypeId);

/// <summary>
/// Represents a position of a specific node in the node kernel.
/// </summary>
/// <param name="Chunk">The target chunk.</param>
/// <param name="Pos">Position in space inside a chunk.</param>
/// <param name="Layer">Layer of the node.</param>
public record struct NodeVoxelHandle(NodeChunkHandle Chunk, Int3 Pos, int Layer);
