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

public record struct AdjacentNodeNet : INodeNet, INodeNetCreator<AdjacentNodeNet>
{
    public float TotalCapacity = 0f;
    
    public NodeNetInternal Net { get; set; }
    
    public void Initialize()
    {
    }

    public void Shutdown()
    {
    }

    public void Merge(IReadOnlySet<NodeNetInternal> nets)
    {
    }

    public void Split(NodeNetInternal parent)
    {
    }

    private AdjacentNodeNet(NodeNetInternal net)
    {
        Net = net;
    }

    public static AdjacentNodeNet CreateNet(NodeNetInternal net)
    {
        return new AdjacentNodeNet(net);
    }
}

public struct AdjacentNodeRule<T> : INodeRule<T>, INodeRuleCreator<AdjacentNodeRule<T>> where T : INode
{
    public IEnumerable<NodeVoxel> Evaluate(INodeKernel solver, NodeVoxel voxel, T nodeData)
    {
        foreach (var offset in Int3Helpers.CardinalOffsets)
        {
            if (solver.TryGetRelative(voxel, offset, voxel.TypeId, out var adjacentVoxel))
                yield return adjacentVoxel;
        }
    }

    public static AdjacentNodeRule<T> CreateRule()
    {
        return new AdjacentNodeRule<T>();
    }
}
