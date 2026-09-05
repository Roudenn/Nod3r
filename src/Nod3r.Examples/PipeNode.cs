using Numos.Maths;
using Nod3r.Types;

namespace Nod3r.Examples;

/// <summary>
/// Pipe that can have up to 6 connections to its cardinal nodes.
/// /// <para>
/// In order for 2 pipes to connect, they must be on the same <see cref="Layer"/>
/// </para>
/// </summary>
public record struct PipeNode() : INode
{
    public float Volume = 1f;

    public PipeDirectionFlags Directions = PipeDirectionFlags.None;

    public byte Layer = 0;
}

[Flags]
public enum PipeDirection : byte
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,
    Forward = 1 << 4,
    Backward = 1 << 5,
}

[Flags]
public enum PipeDirectionFlags : byte
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,
    Forward = 1 << 4,
    Backward = 1 << 5,
}

public static class PipeDirectionExtensions
{
    public static IEnumerable<(Int3, PipeDirection)> ToInt3(this PipeDirectionFlags dir)
    {
        if (dir.HasFlag(PipeDirectionFlags.Forward))
            yield return (Int3.PosX, PipeDirection.Forward);
        if (dir.HasFlag(PipeDirectionFlags.Backward))
            yield return (Int3.NegX, PipeDirection.Backward);
        if (dir.HasFlag(PipeDirectionFlags.Up))
            yield return (Int3.PosZ, PipeDirection.Up);
        if (dir.HasFlag(PipeDirectionFlags.Down))
            yield return (Int3.NegZ, PipeDirection.Down);
        if (dir.HasFlag(PipeDirectionFlags.Right))
            yield return (Int3.PosY, PipeDirection.Right);
        if (dir.HasFlag(PipeDirectionFlags.Left))
            yield return (Int3.NegY, PipeDirection.Left);
    }

    public static PipeDirection GetOpposite(this PipeDirection dir)
    {
        return dir switch
        {
            PipeDirection.Left => PipeDirection.Right,
            PipeDirection.Right => PipeDirection.Left,
            PipeDirection.Up => PipeDirection.Down,
            PipeDirection.Down => PipeDirection.Up,
            PipeDirection.Forward => PipeDirection.Backward,
            PipeDirection.Backward => PipeDirection.Forward,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }
}

public sealed class PipeNodeRule : INodeRule<PipeNode>
{
    public IEnumerable<NodeVoxel> Evaluate(INodeKernel solver, NodeVoxel voxel, PipeNode node)
    {
        foreach (var (offset, dir) in node.Directions.ToInt3())
        {
            if (!solver.TryGetRelative(voxel, offset, voxel.TypeId, out var nearVoxel))
                continue;
            
            if (!solver.TryGetNode<PipeNode>(nearVoxel, out var nearData))
                continue;
            
            if ((nearData.Directions & (PipeDirectionFlags) dir.GetOpposite()) == 0x0)
                continue;
            
            yield return nearVoxel;
        }
    }
}

public struct PipeNodeNetwork : INodeNet, INodeNetCreator<PipeNodeNetwork>
{
    public float TotalCapacity = 0f;
    
    public INodeNetInternal Net { get; set; }
    
    public void Initialize()
    {
    }

    public void Shutdown()
    {
    }

    public void Merge(IReadOnlySet<INodeNetInternal> nets)
    {
    }

    public void Split(INodeNetInternal parent)
    {
    }

    private PipeNodeNetwork(INodeNetInternal net)
    {
        Net = net;
    }

    public static PipeNodeNetwork CreateNet(INodeNetInternal net)
    {
        return new PipeNodeNetwork(net);
    }
}
