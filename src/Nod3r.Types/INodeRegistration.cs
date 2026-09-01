namespace Nod3r.Types;

/// <summary>
/// Interface of the kernel that only provides registration methods.
/// Used by <see cref="NodeConfig"/> to restrict the available methods.
/// </summary>
public interface INodeRegistration
{
    /// <summary>
    /// Amount of node types registered in this solver instance.
    /// </summary>
    int NodeTypeCount { get; }

    /// <summary>
    /// All registered network types ordered by the node registration.
    /// </summary>
    List<Type> RegisteredNetworks { get; }
    
    /// <summary>
    /// Registers a node, its connection rule with other nodes, and a network it creates.
    /// </summary>
    /// <param name="rule">An instance of <see cref="TRule"/> node rule.</param>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TRule"></typeparam>
    /// <typeparam name="TNet"></typeparam>
    void Register<TNode, TRule, TNet>(TRule rule) where TNode : INode where TRule : INodeRule where TNet : INodeNet;
}
