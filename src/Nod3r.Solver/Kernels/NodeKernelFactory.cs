using Nod3r.Solver.Factories;
using Numos.Maths;

namespace Nod3r.Solver.Kernels;

internal sealed class NodeKernelFactory<TNode, TNet>(NodeRule<TNode> rule) : INodeKernelFactory
    where TNode : INode
    where TNet : INodeNet, INodeNetCreator<TNet>
{
    private readonly NodeRule<TNode> _rule = rule;
    
    public INodeKernel Create(Int3 dimensions)
    {
        return new NodeKernel<TNode, TNet>(dimensions);
    }
}
