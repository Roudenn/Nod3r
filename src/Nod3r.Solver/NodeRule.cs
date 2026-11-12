using Numos.Maths;

namespace Nod3r.Solver;

public sealed class NodeRule<T>(
    Int3[] selections,
    short[] layers,
    Func<NodeSelectionArgs<T>, bool> rule) where T : INode
{
    private readonly Int3[] _selections = selections;

    private readonly short[] _layers = layers;
    
    private readonly Func<NodeSelectionArgs<T>, bool> _rule = rule;
}

public sealed class NodeRule(Int3[] selections)
{
    private readonly Int3[] _selections = selections;
}
public readonly record struct NodeSelectionArgs<T>(
    T Current,
    T Neighbor,
    Int3 OtherPos,
    byte CurrentLayer,
    byte OtherLayer) where T : INode;
