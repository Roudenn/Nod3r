using System.Diagnostics.CodeAnalysis;
using Nod3r.Collections;
using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Represents a basic node kernel, which is a system for solving a specific <see cref="INode"/>
/// type or Node ID, paired with a <see cref="INodeNet"/>.
/// <para>
/// Each node kernel stores all of its nodes and node networks,
/// and provides methods to interact with them explicitly.
/// </para>
/// </summary>
/// <remarks>
/// This interface contains public API to interact with a node kernel
/// without having to cast it to a type parameter.
/// </remarks>
internal interface INodeKernel
{
    /// <summary>
    /// Total amount of registered chunks.
    /// </summary>
    int ChunkCount { get; }
    
    /// <summary>
    /// Checks whether a position is initialized in this kernel.
    /// </summary>
    /// <param name="handle">A chunk handle to validate.</param>
    /// <returns>True if the chunk is initialized at this spot, otherwise false.</returns>
    bool HasChunk(Int3 handle);

    /// <summary>
    /// Creates a chunk in the kernel's chunk map.
    /// </summary>
    /// <param name="position">Local chunk map position of the chunk.</param>
    void CreateChunk(Int3 position);
    
    /// <summary>
    /// Removes a node from <see cref="VoxelHandle"/>.
    /// </summary>
    /// <param name="voxel">The target voxel handle in the kernel.</param>
    /// <returns>
    /// True if the node was removed successfully,
    /// false if the target space was already empty.
    /// </returns>
    bool RemoveNode(VoxelHandle voxel);
    
    /// <summary>
    /// Checks whether a <see cref="VoxelHandle"/> contains a node.
    /// </summary>
    /// <param name="voxel">The target voxel handle in the kernel.</param>
    /// <returns>True if a node was found, otherwise false.</returns>
    bool HasNode(VoxelHandle voxel);
    
    bool TryGetRelative(VoxelHandle node, Int3 offset, byte layer, out VoxelHandle relative);
    
    bool TryGetRelative(VoxelHandle node, Int3 offset, out VoxelHandle relative);
}

/// <summary>
/// Interface to interact with a node kernel that controls a specific node type <see cref="T"/>.
/// </summary>
/// <typeparam name="T">Type of node this node kernel controls.</typeparam>
internal interface INodeKernel<T> : INodeKernel where T : INode
{
    /// <summary>
    /// Storage that contains all node data of that kernel instance.
    /// </summary>
    internal NodeStorage<T> NodeStorage { get; set; }
    
    /// <summary>
    /// Sets node data to a <see cref="VoxelHandle"/>.
    /// </summary>
    /// <param name="node">The node data to write into a position.</param>
    /// <param name="voxel">The target voxel position.</param>
    void SetNode(T node, VoxelHandle voxel);
    
    /// <summary>
    /// Adds node data on the lowest available Layer on the target chunk and position.
    /// </summary>
    /// <param name="node">The node data to write into a position.</param>
    /// <param name="chunk">The target chunk.</param>
    /// <param name="pos">The target position in the chunk.</param>
    void AddNode(T node, Int3 chunk, Int3 pos);

    bool TryGetNode(VoxelHandle voxel, [NotNullWhen(true)] out T? node);
    
    void GetChunkData(Int3 chunk, List<(T Data, Int3 Pos)> list, out Int3 dimensions);
}

/// <summary>
/// Interface to interact with a node kernel that controls
/// a specific node network type <see cref="TNet"/>.
/// </summary>
/// <typeparam name="TNet">Type of node network this node kernel controls.</typeparam>
internal interface INodeNetKernel<TNet> : INodeKernel where TNet : INodeNet
{
    /// <summary>
    /// All currently living networks.
    /// </summary>
    internal List<GenId> Nets { get; set; }
    
    /// <summary>
    /// Storage that contains all node network data of that kernel instance.
    /// </summary>
    internal NodeNetStorage<TNet> NodeNetStorage { get; set; }
}

internal interface INodeIDKernel : INodeKernel
{
    void SetNode(VoxelHandle voxel);
    
    void GetChunkData(Int3 chunk, HashSet<Int3> list, out Int3 dimensions);
}
