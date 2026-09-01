namespace Nod3r.Types;

/// <summary>
/// A rule about how a specific node type connects to other nodes.
/// </summary>
public interface INodeRule
{
    IEnumerable<NodeVoxel> Evaluate(INodeSolverKernel solver, NodeVoxel node);
}

/// <inheritdoc/>
/// <para>
/// This is a convenience abstract class that allows to operate on a specific node type more easily.
/// </para>
/// <typeparam name="T">Type of node this rule controls.</typeparam>
public abstract class NodeRule<T> : INodeRule where T : INode
{
    public IEnumerable<NodeVoxel> Evaluate(INodeSolverKernel solver, NodeVoxel node)
    {
        if (!solver.TryGetNode<T>(node, out var nodeData))
            return [];
        
        return EvaluateType(solver, node, nodeData);
    }

    protected abstract IEnumerable<NodeVoxel> EvaluateType(INodeSolverKernel solver, NodeVoxel nodePos, T node);
}
