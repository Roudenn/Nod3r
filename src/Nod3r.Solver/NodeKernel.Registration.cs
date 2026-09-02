using Nod3r.Types;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel
{
    public int NodeTypeCount { get; private set; }
    public List<Type> RegisteredNetworks { get; } = new();
    
    public void Register<TNode, TRule, TNet>(TRule rule, byte layerCapacity = 1)
        where TNode : INode
        where TRule : INodeRule
        where TNet : INodeNet
    {
        RegisteredNetworks.Add(typeof(TNet));
        _nodeTypeIdx.Add(typeof(TNode), new NodeIdx(NodeTypeCount));
        NodeTypeCount++;
        _rules.Add(rule);
        //NodeStorage<TNode>.EnsureLayer(layerCapacity);
    }
}
