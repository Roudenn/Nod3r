using Nod3r.Types;

namespace Nod3r.Solver;

public sealed class NodeRule<TNode, TRule> : NodeRuleHandle
    where TNode : INode
    where TRule : INodeRule<TNode>, INodeRuleCreator<TRule> 
{
    public override IEnumerable<NodeVoxel> Evaluate(INodeKernel kernel, NodeVoxel voxel)
    {
        if (!kernel.TryGetNode(voxel, out TNode? node))
            return [];

        return TRule.CreateRule().Evaluate(kernel, voxel, node);
    }
}
