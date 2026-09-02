using Nod3r.API;
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

public record struct AdjacentNodeNet() : INodeNet, INodeNetCreator<AdjacentNodeNet>
{
    public float TotalCapacity = 0f;
    
    public NodeNetHandler Net { get; set; }
    
    public void Initialize()
    {
    }

    public void Shutdown()
    {
    }

    public void Merge(IReadOnlySet<NodeNetHandler> nets)
    {
    }

    public void Split(NodeNetHandler parent)
    {
    }

    public static AdjacentNodeNet CreateNet()
    {
        return new AdjacentNodeNet();
    }
}
