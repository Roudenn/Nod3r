using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

public sealed class NodeNet<TNode, TNet> : NodeNetInternal where TNode : INode where TNet : INodeNet
{
    private readonly TNet _netImpl;

    public NodeNet(TNet netImpl)
    {
        _netImpl = netImpl;
        netImpl.Net = this;
    }

    public override void Allocate()
    {
        GenId = NodeNetStorage<TNet>.Add(_netImpl);
    }
    
    public override LayerId GetLayerId(INodeKernel kernel, ColumnHandle idx, int layer)
    {
        return NodeStorage<TNode>.GetLayerId(idx, layer);
    }

    public override void Initialize()
    {
        _netImpl.Initialize();
    }

    public override void Shutdown()
    {
        _netImpl.Shutdown();
    }

    public override void Merge(IReadOnlySet<NodeNetInternal> nets)
    {
        _netImpl.Merge(nets);
    }

    public override void Split(NodeNetInternal parent)
    {
        _netImpl.Split(parent);
    }
}
