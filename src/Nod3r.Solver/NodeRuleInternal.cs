using Nod3r.Types;

namespace Nod3r.Solver;

internal abstract class NodeRuleInternal
{
    public abstract IEnumerable<NodeVoxel> Evaluate(INodeKernel kernel, NodeVoxel voxel);
}
