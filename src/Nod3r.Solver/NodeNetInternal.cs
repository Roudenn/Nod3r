using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// A wrapper around a node network that allows to access it in abstract context.
/// </summary>
internal abstract class NodeNetInternal : INodeNetInternal
{
    /// <summary>
    /// References to nodes that are connected to this node network.
    /// </summary>
    public HashSet<LayerId> Nodes = new();

    public GenId GenId;

    public abstract void Allocate(NodeKernel kernel);

    /// <summary>
    /// Initialize function that is called after this node group was properly set up.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Last function that is called after every single node
    /// that this node network controlled has been removed.
    /// This is the last called function before the network is released.
    /// </summary>
    public abstract void Shutdown();

    /// <summary>
    /// <para>
    /// Merges a set of node networks into this network.
    /// </para>
    /// <para>
    /// This method is called right before Shutdown and release of every network in the set.
    /// </para>
    /// </summary>
    public abstract void Merge(IReadOnlySet<INodeNetInternal> nets);

    /// <summary>
    /// Called on a newly created node network after its initialization
    /// that was split from the <see cref="parent"/> network.
    /// </summary>
    public abstract void Split(INodeNetInternal parent);
}
