using Nod3r.Types;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel
{
    public void Register<TNode, TRule, TNet>(TRule rule, byte layerCapacity = 1)
        where TNode : INode
        where TRule : INodeRule
        where TNet : INodeNet
    {
        _rules.Add(rule);
        NodeIdxStorage.Register<TNode, TNet>();
        NodeStorage<TNode>.EnsureLayerCapacity(layerCapacity);
    }
}
