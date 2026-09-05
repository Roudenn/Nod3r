using Nod3r.Types;

namespace Nod3r.Solver;

internal sealed class NodeNet<TNet> : NodeNetInternal where TNet : INodeNet, INodeNetCreator<TNet>
{
    private readonly TNet _netImpl;

    public NodeNet()
    {
        _netImpl = TNet.CreateNet(this);
    }

    public override void Allocate(NodeKernel kernel)
    {
        GenId = kernel.GetNetStorageTyped<TNet>().Add(_netImpl);
    }

    public override void Initialize()
    {
        _netImpl.Initialize();
    }

    public override void Shutdown()
    {
        _netImpl.Shutdown();
    }

    public override void Merge(IReadOnlySet<INodeNetInternal> nets)
    {
        _netImpl.Merge(nets);
    }

    public override void Split(INodeNetInternal parent)
    {
        _netImpl.Split(parent);
    }
}
