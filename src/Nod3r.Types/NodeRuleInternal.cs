namespace Nod3r.Types;

public abstract class NodeRuleInternal
{
    public abstract IEnumerable<NodeVoxel> Evaluate(INodeKernel kernel, NodeVoxel voxel);
}
