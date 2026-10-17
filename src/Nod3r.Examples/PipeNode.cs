using Nod3r.API;
using Nod3r.Collections;
using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.Examples;

/// <summary>
/// Pipe that can have up to 6 connections to its cardinal nodes.
/// /// <para>
/// In order for 2 pipes to connect, they must be on the same <see cref="Layer"/>
/// </para>
/// </summary>
public record struct PipeNode() : INode, INodeAutoRegistration<PipeNode>
{
    public float Volume = 1f;

    public PipeDirectionFlags Directions = PipeDirectionFlags.None;

    public static NodeRule<PipeNode> ConnectionRule =>
        NodeRuleBuilder.Create()
            .IncludeCartesian()
            .Compile<PipeNode>(
                args
                => (args.Current.Directions & (PipeDirectionFlags)PipeDirectionExtensions.GetDirection(args.OtherPos)) == 0x0
                   && (args.Neighbor.Directions & (PipeDirectionFlags)PipeDirectionExtensions.GetDirection(new Int3() - args.OtherPos)) == 0x0);
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

    public static PipeDirection GetDirection(Int3 dir)
    {
        if (dir == Int3.PosX)
            return PipeDirection.Forward;
        if (dir == Int3.NegX)
            return PipeDirection.Backward;
        if (dir == Int3.PosZ)
            return PipeDirection.Up;
        if (dir == Int3.NegZ)
            return PipeDirection.Down;
        if (dir == Int3.PosY)
            return PipeDirection.Right;
        if (dir == Int3.NegY)
            return PipeDirection.Left;

        return PipeDirection.None;
    }
}

public struct PipeNodeNetwork() : INodeNet<PipeNode, PipeNodeNetwork>, INodeNetCreator<PipeNodeNetwork>
{
    public float TotalCapacity = 0f;

    public HashSet<LayerId> Nodes { get; } = new();

    public void Initialize()
    {
    }

    public void Shutdown()
    {
    }

    public void Merge(IReadOnlySet<PipeNodeNetwork> nets)
    {
    }

    public void Split(PipeNodeNetwork parent)
    {
    }

    public static PipeNodeNetwork CreateNet()
    {
        return new PipeNodeNetwork();
    }
}
