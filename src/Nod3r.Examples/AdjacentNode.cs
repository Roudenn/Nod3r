using Nod3r.API;
using Nod3r.Solver;
using Nod3r.Types;

namespace Nod3r.Examples;

/// <summary>
/// A simple node that connects to all cardinal neighbors.
/// </summary>
public record struct AdjacentNode() : INode
{
    public float Capacity = 1f;
}

public sealed class AdjacentNodeRule : NodeRule<AdjacentNode>
{
    protected override IEnumerable<NodeVoxel> EvaluateType(INodeKernel solver, NodeVoxel voxel, AdjacentNode nodeData)
    {
        foreach (var offset in Int3Helpers.CardinalOffsets)
        {
            if (solver.TryGetRelative(voxel, offset, voxel.TypeId, out var adjacentVoxel))
                yield return adjacentVoxel;
        }
    }
}

public sealed class AdjacentNodeNetwork : NodeNet<AdjacentNode>
{
    public float TotalCapacity = 0f;
}
