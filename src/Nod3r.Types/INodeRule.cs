namespace Nod3r.Types;

/// <summary>
/// A rule about how a specific node type connects to other nodes.
/// </summary>
/// <remarks>
/// This is a marker struct specifically for <see cref="INodeRuleCreator{T}"/>.
/// </remarks>
public interface INodeRule;

/// <summary>
/// A rule about how a specific node type connects to other nodes.
/// </summary>
/// <typeparam name="T">Type of node this rule controls.</typeparam>
public interface INodeRule<in T> : INodeRule where T : INode
{
    IEnumerable<NodeVoxel> Evaluate(INodeKernel solver, NodeVoxel voxel, T node);
}
