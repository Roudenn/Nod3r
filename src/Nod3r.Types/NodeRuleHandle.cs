namespace Nod3r.Types;

public abstract class NodeRuleHandle
{
    public abstract IEnumerable<NodeVoxel> Evaluate(INodeKernel kernel, NodeVoxel voxel);
}
