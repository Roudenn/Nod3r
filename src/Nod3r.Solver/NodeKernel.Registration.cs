using Nod3r.Types;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel
{
    public List<Type> RegisteredNetworks { get; } = new();
    
    public void Register<TNode, TRule, TNet>(TRule rule, byte layerCapacity = 1)
        where TNode : INode
        where TRule : INodeRule
        where TNet : INodeNet
    {
        RegisteredNetworks.Add(typeof(TNet));
        NodeIdxStorage.Register<TNode>();
        _rules.Add(rule);
        //NodeStorage<TNode>.EnsureLayer(layerCapacity);
    }
}
