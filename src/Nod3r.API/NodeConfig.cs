using Nod3r.Solver;

namespace Nod3r.API;

/// <summary>
/// A config for initializing a <see cref="NodeSimulation"/> instance.
/// </summary>
/// <param name="subs">Delegate that specifies which node types and IDs to register.</param>
public sealed class NodeConfig(Action<INodeRegistration> subs)
{
    public readonly Action<INodeRegistration> RegistrationDelegate = subs;
}

/// <summary>
/// Interface of the kernel that only provides registration methods.
/// Used by <see cref="NodeConfig"/> to restrict the available methods.
/// </summary>
public interface INodeRegistration
{
    /// <summary>
    /// Registers an <see cref="INode"/> type, its <see cref="INodeRule{T}"/> connection rule
    /// with other nodes, and an <see cref="INodeNet"/> network it creates.
    /// </summary>
    /// <param name="rule">
    /// Node rule that defines how the registered node type should connect to its neighbors.
    /// See <see cref="NodeRuleBuilder"/> for more details.
    /// </param>
    /// <param name="layerCapacity">
    /// Default layer capacity for node type <see cref="TNode"/>.
    /// If multiple nodes are going to be placed frequently in a single voxel,
    /// it's recommended to set this value to the most common and large amount of nodes
    /// that a single voxel may have to avoid automatic array resizes.
    /// </param>
    /// <typeparam name="TNode">Type of the registered node.</typeparam>
    /// <typeparam name="TNet">Type of the registered node network that control type <see cref="TNode"/>.</typeparam>
    /// <seealso cref="NodeRuleBuilder"/>
    void Register<TNode, TNet>(NodeRule<TNode> rule, byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>;
    
    /// <summary>
    /// Registers an <see cref="INode"/> type, its <see cref="NodeRule{T}"/> connection rule
    /// with other nodes, and an <see cref="INodeNet"/> network it creates.
    /// </summary>
    /// <param name="layerCapacity">
    /// Default layer capacity for node type <see cref="TNode"/>.
    /// If multiple nodes are going to be placed frequently in a single voxel,
    /// it's recommended to set this value to the most common and large amount of nodes
    /// that a single voxel may have to avoid automatic array resizes.
    /// </param>
    /// <typeparam name="TNode">Type of the registered node.</typeparam>
    /// <typeparam name="TNet">Type of the registered node network that control type <see cref="TNode"/>.</typeparam>
    /// <seealso cref="NodeRuleBuilder"/>
    void Register<TNode, TNet>(byte layerCapacity = 1)
        where TNode : INode, INodeAutoRegistration<TNode>
        where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>
    => Register<TNode, TNet>(TNode.ConnectionRule, layerCapacity);

    /// <summary>
    /// Registers a node ID, its <see cref="INodeRule{T}"/> connection rule
    /// with other nodes, and an <see cref="INodeNet"/> network it creates.
    /// <para>
    /// ID-based nodes don't hold any actual data because they are not types.
    /// Instead, they're stored directly as bytes in chunks, which improves memory usage significantly.
    /// </para>
    /// <para>
    /// ID nodes don't support node layers.
    /// </para>
    /// </summary>
    /// <param name="rule">
    /// Node rule that defines how the registered node type should connect to its neighbors.
    /// See <see cref="NodeRuleBuilder"/> for more details.
    /// </param>
    /// <param name="id">ID of this node </param>
    /// <typeparam name="TNet">Type of the registered node network.</typeparam>
    void RegisterID<TNet>(NodeRule rule, int id)
        where TNet : INodeNet, INodeNetCreator<TNet>;
}
