using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// A custom object that creates new instances of <see cref="INodeNet"/> types.
/// </summary>
public abstract class NodeRuleFactory
{
    public abstract NodeRuleInternal Create();
}

public sealed class NodeRuleFactory<TNode, TRule> : NodeRuleFactory
    where TNode : INode
    where TRule : INodeRule<TNode>, INodeRuleCreator<TRule>
{
    public override NodeRuleInternal Create()
    {
        return new NodeRule<TNode, TRule>();
    }
}
