using System.Diagnostics.CodeAnalysis;
using Nod3r.Collections;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Represents a basic node kernel, which is a system for solving a specific <see cref="INode"/>
/// type or Node ID, paired with a <see cref="INodeNet"/> and <see cref="INodeRule{T}"/>.
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
    bool HasChunk(Int3 position);

    void CreateChunk(Int3 position, int width, int height, int depth);
    
    /// <summary>
    /// Removes a node from <see cref="NodeVoxelHandle"/>.
    /// </summary>
    /// <param name="voxel">The target voxel handle in the kernel.</param>
    /// <returns>
    /// True if the node was removed successfully,
    /// false if the target space was already empty.
    /// </returns>
    bool RemoveNode(NodeVoxelHandle voxel);
    
    /// <summary>
    /// Checks whether a <see cref="NodeVoxelHandle"/> contains a node.
    /// </summary>
    /// <param name="voxel">The target voxel handle in the kernel.</param>
    /// <returns>True if a node was found, otherwise false.</returns>
    bool HasNode(NodeVoxelHandle voxel);
    
    bool TryGetRelative(NodeVoxelHandle node, Int3 offset, int layer, out NodeVoxelHandle relative);
    
    bool TryGetRelative(NodeVoxelHandle node, Int3 offset, out NodeVoxelHandle relative);
}

/// <summary>
/// Interface to interact with a node kernel that controls a specific node type <see cref="T"/>.
/// </summary>
/// <typeparam name="T">Type of node this node kernel controls.</typeparam>
internal interface INodeKernel<T> : INodeKernel
    where T : INode
{
    internal List<GenId> Nets { get; set; }
    
    /// <summary>
    /// Storage that contains all node data of that kernel instance.
    /// </summary>
    internal NodeStorage<T> NodeStorage { get; set; }
    
    /// <summary>
    /// Sets node data to a <see cref="NodeVoxelHandle"/>.
    /// </summary>
    /// <param name="node">The node data to write into a position.</param>
    /// <param name="voxel">The target voxel position.</param>
    void SetNode(T node, NodeVoxelHandle voxel);
    
    /// <summary>
    /// Adds node data on the lowest available Layer on the target chunk and position.
    /// </summary>
    /// <param name="node">The node data to write into a position.</param>
    /// <param name="chunk">The target chunk.</param>
    /// <param name="pos">The target position in the chunk.</param>
    void AddNode(T node, NodeChunkHandle chunk, Int3 pos);

    bool TryGetNode(NodeVoxelHandle voxel, [NotNullWhen(true)] out T? node);
    
    bool TryGetNode(NodeChunkHandle chunk, Int3 pos, int layer, [NotNullWhen(true)] out T? node);
}

/// <summary>
/// Interface to interact with a node kernel that controls a specific node type <see cref="TNode"/>
/// and a node network type <see cref="TNet"/>.
/// </summary>
/// <typeparam name="TNode">Type of node this node kernel controls.</typeparam>
/// <typeparam name="TNet">Type of node network this node kernel controls.</typeparam>
internal interface INodeKernel<TNode, TNet> : INodeKernel<TNode>
    where TNode : INode
    where TNet : INodeNet
{
    /// <summary>
    /// Storage that contains all node network data of that kernel instance.
    /// </summary>
    internal NodeNetStorage<TNet> NodeNetStorage { get; set; }
}
