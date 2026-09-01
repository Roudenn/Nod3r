using Nod3r.Collections;

namespace Nod3r.Types;

/// <summary>
/// Represents a network of connected <see cref="INode"/>s.
/// </summary>
/// <para>
/// Node networks are the final result of produced by the <see cref="INodeSolverKernel"/>.
/// They contain references to all nodes that are connected between each other according to the specified <see cref="INodeRule"/>s.
/// Each network contains the data that is shared between all nodes.
/// </para>
/// <para>
/// Node networks are created when an isolated node or a range of nodes is added to a chunk after the next rebuild.
/// After creation, networks can be merged with one or multiple other node networks in case if they become connected,
/// and be destroyed in case if all remaining nodes that they hold are removed.
/// </para>
public interface INodeNet
{
    /// <summary>
    /// References to nodes that are connected to this node network.
    /// </summary>
    HashSet<GenId> Nodes { get; }
    
    /// <summary>
    /// Initialize function that is called after this node group was properly set up.
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// Last function that is called after every single node
    /// that this node network controlled has been removed.
    /// This is the last called function before the network is released.
    /// </summary>
    void Shutdown();
    
    /// <summary>
    /// <para>
    /// Merges a set of node networks into this network.
    /// </para>
    /// <para>
    /// This method is called right before Shutdown and release of every network in the set.
    /// </para>
    /// </summary>
    void Merge(IReadOnlySet<INodeNet> nets);

    /// <summary>
    /// Called on a newly created node network after its initialization
    /// that was split from the <see cref="parent"/> network.
    /// </summary>
    void Split(INodeNet parent);
}
