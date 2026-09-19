using Numos.Collections;
using Numos.Maths;

namespace Nod3r.Solver;

// TODO Numos copy-paste
/// <summary>
/// Stores ID nodes of a node kernel as 3D chunks.
/// </summary>
internal sealed class NodeIDChunk
{
    // TODO implement byte weaving
    public FlatArray<bool> Bytes;

    /// <summary>
    /// Dimensions of this chunk.
    /// </summary>
    public Int3 Dimensions;

    /// <summary>
    /// Position of this chunk in the chunk grid.
    /// </summary>
    public Int3 GridPosition;
    
    public NodeIDChunk(
        int width = NodeChunkConstants.DefaultWidth,
        int height = NodeChunkConstants.DefaultHeight,
        int depth = NodeChunkConstants.DefaultDepth)
    {
        Dimensions = new Int3(width, height, depth);
        Bytes = new FlatArray<bool>(new bool[Dimensions.X * Dimensions.Y * Dimensions.Z], Dimensions);
    }
}
