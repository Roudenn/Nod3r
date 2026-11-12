using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Defines a connection rule for a specific node type.
/// </summary>
/// <param name="selections">Relative positions to check around the nodes position.</param>
/// <param name="layers">Relative layers to check around the current node's layer.</param>
/// <param name="rule">A condition that checks whether the current node can connect to the other one.</param>
/// <typeparam name="T">Type of the node.</typeparam>
public sealed class NodeRule<T>(
    Int3[] selections,
    short[] layers,
    Func<NodeSelectionArgs<T>, bool> rule) where T : INode
{
    private readonly Int3[] _selections = selections;

    private readonly short[] _layers = layers;
    
    private readonly Func<NodeSelectionArgs<T>, bool> _rule = rule;
}

/// <summary>
/// Defines a connection rule for an arbitrary ID node.
/// </summary>
/// <param name="selections">Relative positions to check around the nodes position.</param>
public sealed class NodeRule(Int3[] selections)
{
    private readonly Int3[] _selections = selections;
}

/// <summary>
/// Arguments for a condition of a <see cref="NodeRule{T}"/>.
/// </summary>
/// <param name="Current">Current node data.</param>
/// <param name="Neighbor">Neighbor's node data</param>
/// <param name="OtherPos">
/// Relative position of the other node. Current node is always considered to be at (0,0,0).
/// </param>
/// <param name="CurrentLayer">Layer of the current node.</param>
/// <param name="OtherLayer">Layer of the neighbor.</param>
/// <typeparam name="T">Type of both connecting nodes.</typeparam>
public readonly record struct NodeSelectionArgs<T>(
    T Current,
    T Neighbor,
    Int3 OtherPos,
    byte CurrentLayer,
    byte OtherLayer) where T : INode;
