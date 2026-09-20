using Nod3r.API;
using Nod3r.Types;

namespace Nod3r.Examples;

public struct AdjacentNodeRule<T> : INodeRule<T>, INodeRuleCreator<AdjacentNodeRule<T>> where T : INode
{
    public IEnumerable<NodeVoxel> Evaluate(INodeSolver solver, NodeVoxelHandle voxel, T nodeData)
    {
        foreach (var offset in Int3Helpers.CardinalOffsets)
        {
            if (solver.TryGetRelative<T>(voxel, offset, out var adjacentVoxel))
                yield return adjacentVoxel;
        }
    }

    public static AdjacentNodeRule<T> CreateRule()
    {
        return new AdjacentNodeRule<T>();
    }
}

public struct AdjacentNodeRule : INodeRuleID, INodeRuleCreator<AdjacentNodeRule>
{
    public IEnumerable<NodeVoxel> Evaluate(INodeSolver solver, NodeVoxel voxel)
    {
        foreach (var offset in Int3Helpers.CardinalOffsets)
        {
            if (solver.TryGetRelative(voxel, offset, out var adjacentVoxel))
                yield return adjacentVoxel;
        }
    }

    public static AdjacentNodeRule CreateRule()
    {
        return new AdjacentNodeRule();
    }
}
