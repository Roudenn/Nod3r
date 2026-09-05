using Nod3r.Types;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel
{
    public void Register<TNode, TNet, TRule>(byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet, INodeNetCreator<TNet>
        where TRule : INodeRule<TNode>, INodeRuleCreator<TRule>
    {
        _ruleFactories.Add(new NodeRuleFactory<TNode, TRule>());
        _nodeFactories.Add(new NodeNetFactory<TNet>());
        NodeIdxStorage.Register<TNode, TNet>();
        Array.Resize(ref _nodeStorages, NodeIdxStorage.Count);
        _nodeStorages[NodeIdxStorage.Count].EnsureLayerCapacity(layerCapacity);
    }
}
