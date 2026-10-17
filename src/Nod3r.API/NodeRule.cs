using JetBrains.Annotations;
using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.API;

/// <summary>
/// Static helper class for creating <see cref="NodeVoxelSelectionBuilder"/> instances.
/// </summary>
/// <seealso cref="NodeRule"/>
/// <seealso cref="NodeRule{T}"/>
public static class NodeRuleBuilder
{
    /// <summary>
    /// Creates a default <see cref="NodeVoxelSelectionBuilder"/> that can be used to create node rules.
    /// </summary>
    /// <seealso cref="NodeRule"/>
    /// <seealso cref="NodeRule{T}"/>
    [Pure, PublicAPI]
    public static NodeVoxelSelectionBuilder Create()
    {
        return new NodeVoxelSelectionBuilder();
    }

    [PublicAPI]
    public static NodeRule Empty => new([]);
}

public static class NodeRuleBuilder<T> where T : INode
{
    [PublicAPI]
    public static NodeRule<T> Empty => new([], [], _ => true);
}

/// <summary>
/// Helper class for specifying the rules and creating
/// <see cref="NodeRule"/> and <see cref="NodeRule{T}"/> instances.
/// </summary>
public sealed class NodeVoxelSelectionBuilder
{
    /// <summary>
    /// Relative positions to be checked by the selection.
    /// </summary>
    private readonly HashSet<Int3> _selections = new();

    /// <summary>
    /// Relative layer offsets to be checked by the selection.
    /// </summary>
    private readonly HashSet<short> _layers = [0];
    
    /// <summary>
    /// Includes all direct neighbors of a cell to this selection.
    /// </summary>
    [Pure, PublicAPI]
    public NodeVoxelSelectionBuilder IncludeCartesian()
    {
        _selections.UnionWith(Int3Helpers.CardinalOffsets);
        return this;
    }
    
    /// <summary>
    /// Includes a span of specified relative positions to this selection.
    /// </summary>
    [Pure, PublicAPI]
    public NodeVoxelSelectionBuilder Select(params ReadOnlySpan<Int3> selections)
    {
        foreach (var pos in selections)
        {
            _selections.Add(pos);
        }
        
        return this;
    }
    
    [Pure, PublicAPI]
    public NodeVoxelSelectionBuilder AddLayers(params ReadOnlySpan<short> layers)
    {
        foreach (var layer in layers)
        {
            _layers.Add(layer);
        }
        
        return this;
    }
    
    [Pure, PublicAPI]
    public NodeVoxelSelectionBuilder SetLayers(params ReadOnlySpan<short> layers)
    {
        ArgumentOutOfRangeException.ThrowIfZero(layers.Length, nameof(layers));
        
        _layers.Clear();
        foreach (var layer in layers)
        {
            _layers.Add(layer);
        }
        
        return this;
    }

    /// <summary>
    /// Compiles a Typed node rule for 2 nodes of the same type.
    /// It can check whether the two nodes should be connected
    /// by comparing their internal data by providing a delegate.
    /// </summary>
    /// <param name="rule">The delegate that checks whether the 2 nodes can connect or not.</param>
    /// <typeparam name="T">Type of node this rule controls.</typeparam>
    /// <returns>A new <see cref="NodeRule{T}"/> instance.</returns>
    [Pure, PublicAPI]
    public NodeRule<T> Compile<T>(Func<NodeSelectionArgs<T>, bool>? rule = null) where T : INode
    {
        rule ??= _ => true;
        var selections = new Int3[_selections.Count];
        _selections.CopyTo(selections, 0);
        var layers = new short[_layers.Count];
        _layers.CopyTo(layers, 0);
        return new NodeRule<T>(selections, layers, rule);
    }

    [Pure, PublicAPI]
    public NodeRule Compile()
    {
        var arr = new Int3[_selections.Count];
        _selections.CopyTo(arr, 0);
        return new NodeRule(arr);
    }
}

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
