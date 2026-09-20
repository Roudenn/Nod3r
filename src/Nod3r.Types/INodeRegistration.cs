namespace Nod3r.Types;

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
    /// <param name="layerCapacity">
    /// Default layer capacity for node type <see cref="TNode"/>.
    /// If multiple nodes are going to be placed frequently in a single voxel,
    /// it's recommended to set this value to the most common and large amount of nodes
    /// that a single voxel may have to avoid automatic array resizes.
    /// </param>
    /// <typeparam name="TNode">Type of the registered node.</typeparam>
    /// <typeparam name="TRule">Type of the registered rule that controls <see cref="TNode"/>.</typeparam>
    /// <typeparam name="TNet">Type of the registered node network that control type <see cref="TNode"/>.</typeparam>
    void Register<TNode, TNet, TRule>(byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>
        where TRule : INodeRule<TNode>, INodeRuleCreator<TRule>;
    
    /// <summary>
    /// Registers a node ID, its <see cref="INodeRule{T}"/> connection rule
    /// with other nodes, and an <see cref="INodeNet"/> network it creates.
    /// </summary>
    /// <para>
    /// ID-based nodes don't hold any actual data because they are not types.
    /// Instead, they're stored directly as bytes in chunks, which improves the memory usage significantly.
    /// </para>
    /// <para>
    /// ID nodes don't support node layers at the current moment.
    /// </para>
    /// <typeparam name="TRule">Type of the registered rule.</typeparam>
    /// <typeparam name="TNet">Type of the registered node network.</typeparam>
    void RegisterID<TNet, TRule>(int id, out NodeIdx registered)
        where TNet : INodeNet, INodeNetCreator<TNet>
        where TRule : INodeRule, INodeRuleCreator<TRule>;
}
