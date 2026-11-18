using Nod3r.Collections;
using Numos.Maths;

namespace Nod3r.Solver.Chunks;

/// <summary>
/// Stores ID nodes of a node kernel as 3D chunks.
/// </summary>
internal sealed class NodeIDChunk
{
    public FlatByteArray Nodes;

    /// <summary>
    /// Dimensions of this chunk.
    /// </summary>
    public Int3 Dimensions;

    /// <summary>
    /// Position of this chunk in the chunk grid.
    /// </summary>
    public Int3 GridPosition;
    
    public NodeIDChunk(Int3 gridPosition, Int3 dimensions)
    {
        Dimensions = dimensions;
        GridPosition = gridPosition;
        Nodes = new FlatByteArray(Dimensions);
    }
}
