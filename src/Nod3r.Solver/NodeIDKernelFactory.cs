using Numos.Maths;

namespace Nod3r.Solver;

internal sealed class NodeIDKernelFactory<TNet>(int id, NodeRule rule) : INodeIDKernelFactory
    where TNet : INodeNet, INodeNetCreator<TNet>
{
    public int ID { get; } = id;
    
    public INodeIDKernel Create(Int3 dimensions)
    {
        return new NodeIDKernel<TNet>(ID, rule, dimensions);
    }
}
