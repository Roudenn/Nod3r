using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver.Tests;

public struct DummyNodeNet<T>() : INodeNet<T, DummyNodeNet<T>>, INodeNetCreator<DummyNodeNet<T>> where T : INode
{
    public HashSet<LayerId> Nodes { get; } = new();
        
    public void Initialize()
    {
    }

    public void Shutdown()
    {
    }

    public void Merge(IReadOnlySet<DummyNodeNet<T>> nets)
    {
    }

    public void Split(DummyNodeNet<T> parent)
    {
    }

    public static DummyNodeNet<T> CreateNet()
    {
        return new DummyNodeNet<T>();
    }
}

public struct DummyNodeRule<TNode> : INodeRule<TNode>, INodeRuleCreator<DummyNodeRule<TNode>>
    where TNode : INode
{
    public IEnumerable<NodeVoxel> Evaluate(INodeSolver solver, NodeVoxelHandle voxel, TNode node)
    {
        return [];
    }
        
    public static DummyNodeRule<TNode> CreateRule()
    {
        return new DummyNodeRule<TNode>();
    }
}
    
public struct DummyNodeRuleID : INodeRuleID, INodeRuleCreator<DummyNodeRuleID>
{
    public IEnumerable<NodeVoxel> Evaluate(INodeSolver solver, NodeVoxel voxel)
    {
        return [];
    }
        
    public static DummyNodeRuleID CreateRule()
    {
        return new DummyNodeRuleID();
    }
}

public struct TestNode1 : INode;
public struct TestNode2 : INode;
public struct TestNode3 : INode;
public struct TestNode4 : INode;
public struct TestNode5 : INode;
public struct TestNode6 : INode;
public struct TestNode7 : INode;
public struct TestNode8 : INode;
