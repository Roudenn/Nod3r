namespace Nod3r.Types;

/// <summary>
/// Interface of the kernel that only provides registration methods.
/// Used by <see cref="NodeConfig"/> to restrict the available methods.
/// </summary>
public interface INodeRegistration
{
    /// <summary>
    /// Registers a node, its connection rule with other nodes, and a network it creates.
    /// </summary>
    /// <param name="rule">An instance of <see cref="TRule"/> node rule.</param>
    /// <param name="layerCapacity">
    /// Default layer capacity for node type <see cref="TNode"/>.
    /// If multiple nodes are going to be placed frequently in a single voxel,
    /// it's recommended to set this value to the most common and large amount of nodes
    /// that a single voxel may have to avoid automatic array resizes.
    /// </param>
    /// <typeparam name="TNode">Type of the registered node.</typeparam>
    /// <typeparam name="TRule">Type of the registered rule that controls <see cref="TNode"/>.</typeparam>
    /// <typeparam name="TNet">Type of the registered node network that control type <see cref="TNode"/>.</typeparam>
    void Register<TNode, TNet, TRule>(TRule rule, byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet, INodeNetCreator<TNet>
        where TRule : class, INodeRule;
}
