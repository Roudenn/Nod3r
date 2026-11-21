using Nod3r.Collections;
using Numos.Collections;
using Numos.Maths;

namespace Nod3r.Solver.Chunks;

/// <summary>
/// Stores ID nodes of a node kernel as 3D chunks.
/// </summary>
internal sealed class NodeIDChunk
{
    /// <summary>
    /// Flags at every node position that specifies
    /// whether a node is placed here or not.
    /// </summary>
    public FlatByteArray Nodes;

    /// <summary>
    /// Data about the network of every node in the chunk.
    /// </summary>
    // TODO benchmark this and consider a 3D-voxel tree data structure to optimize for memory
    public FlatArray<GenId> Networks;

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
        Networks = new FlatArray<GenId>(new GenId[Dimensions.X * Dimensions.Y * Dimensions.Z], Dimensions);
    }
}
