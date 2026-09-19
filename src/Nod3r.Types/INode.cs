namespace Nod3r.Types;

/// <summary>
/// Basic interface for all nodes that can be stored in a <see cref="INodeSolver"/>.
/// </summary>
/// <para>
/// Nodes are "atoms" of a graph. In Nod3r, nodes are stored in a voxel grid,
/// where each node can connect to other nodes according to some <see cref="INodeRule{T}"/>.
/// </para>
/// <para>
/// Each <see cref="INode"/> contains data that is specific for every single part of a graph.
/// For example, this can be a direction in which a pipe is facing
/// (so it can only connect on specific sides depending on how it's rotated).
/// </para>
/// <para>
/// When some data that is used for node connection rules is changed,
/// the node should be marked as dirty and the node network has to be rebuilt to be correct.
/// </para>
public interface INode;
