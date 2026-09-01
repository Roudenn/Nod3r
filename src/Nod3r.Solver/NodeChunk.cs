using Nod3r.Collections;
using Numos.Collections;
using Numos.Maths;

namespace Nod3r.Solver;

// TODO Numos copy-paste
internal sealed class NodeChunk
{
    /// <summary>
    /// Array that stores <see cref="GenId"/>s that reference a node
    /// of a certain type in the <see cref="NodeStorage{T}"/>.
    /// </summary>
    public FlatArray<GenIdx> Chunk;

    /// <summary>
    /// Dimensions of this chunk.
    /// </summary>
    public Int3 Dimensions;
    
    
    
    public NodeChunk(
        int width = NodeChunkConstants.DefaultWidth,
        int height = NodeChunkConstants.DefaultHeight,
        int depth = NodeChunkConstants.DefaultDepth)
    {
        Dimensions = new Int3(width, height, depth);
        var array = new GenIdx[width * height * depth];
        Array.Fill(array, GenIdx.Invalid);
        Chunk = new FlatArray<GenIdx>(array, Dimensions);
    }
}
