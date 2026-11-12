using JetBrains.Annotations;
using Nod3r.Solver;

namespace Nod3r.API.Rules;

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

    /// <summary>
    /// An empty ID node rule with no neighbors.
    /// </summary>
    [PublicAPI]
    public static NodeRule Empty => new([]);
}

/// <summary>
/// Static helper class for typed node rules.
/// </summary>
public static class NodeRuleBuilder<T> where T : INode
{
    /// <summary>
    /// An empty typed node rule with no neighbors.
    /// </summary>
    [PublicAPI]
    public static NodeRule<T> Empty => new([], [], _ => true);
}
