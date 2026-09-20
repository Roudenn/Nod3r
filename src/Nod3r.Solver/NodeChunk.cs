using Nod3r.Collections;
using Numos.Collections;
using Numos.Maths;

namespace Nod3r.Solver;

// TODO Numos copy-paste
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

    /// <summary>
    /// Dimensions of this chunk.
    /// </summary>
    public Int3 Dimensions;

    /// <summary>
    /// Position of this chunk in the chunk grid.
    /// </summary>
    public Int3 GridPosition;
    
    public NodeChunk(
        int width = NodeChunkConstants.DefaultWidth,
        int height = NodeChunkConstants.DefaultHeight,
        int depth = NodeChunkConstants.DefaultDepth)
    {
        Dimensions = new Int3(width, height, depth);
        var length = Dimensions.X * Dimensions.Y * Dimensions.Z;
        Handles = new FlatArray<ColumnHandle>(new ColumnHandle[length], Dimensions);
        for (int i = 0; i < length; i++)
        {
            Handles[i] = ColumnHandle.Invalid;
        }
    }
}
