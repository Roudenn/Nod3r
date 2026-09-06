using Nod3r.Collections;
using Nod3r.Types;
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
    public FlatArray<ColumnHandle>[] Chunks;

    /// <summary>
    /// Dimensions of this chunk.
    /// </summary>
    public Int3 Dimensions;

    /// <summary>
    /// Position of this chunk in the chunk grid.
    /// </summary>
    public Int3 GridPosition;
    
    public NodeChunk(
        List<NodeIdx> registeredTypes,
        int width = NodeChunkConstants.DefaultWidth,
        int height = NodeChunkConstants.DefaultHeight,
        int depth = NodeChunkConstants.DefaultDepth)
    {
        Dimensions = new Int3(width, height, depth);
        Chunks = new FlatArray<ColumnHandle>[registeredTypes.Count];
        
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i] = CreateArray();
        }
    }

    public FlatArray<ColumnHandle> CreateArray()
    {
        var array = new ColumnHandle[Dimensions.X * Dimensions.Y * Dimensions.Z];
        Array.Fill(array, ColumnHandle.Invalid);
        return new FlatArray<ColumnHandle>(array, Dimensions);
    }
}
