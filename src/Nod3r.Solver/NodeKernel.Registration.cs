using Nod3r.Types;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel
{
    public void Register<TNode, TNet, TRule>(TRule rule, byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet, INodeNetCreator<TNet>
        where TRule : class, INodeRule
    {
        _rules.Add(rule);
        _nodeFactories.Add(new NodeNetFactory<TNode, TNet>());
        NodeIdxStorage.Register<TNode, TNet>();
        NodeStorage<TNode>.EnsureLayerCapacity(layerCapacity);
    }
}
