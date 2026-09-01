using Numos.Maths;

namespace Nod3r.Types;

/// <summary>
/// Represents a position of a specific node in the simulation.
/// </summary>
/// <param name="Pos">Position in space inside a chunk.</param>
/// <param name="TypeId">Index type of node.</param>
public record struct NodeVoxel(Int3 Pos, Int3 Chunk, NodeIdx TypeId, int Layer);
