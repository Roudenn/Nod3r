using Nod3r.API;
using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver.Tests;

/// <summary>
/// Test for different node type registration scenarios.
/// </summary>
[TestFixture]
public sealed class NodeRegistrationTest
{
    // TODO check if chunk initialization is correct
    
    /// <summary>
    /// Simple registration of a single solver instance with multiple node types.
    /// </summary>
    [Test]
    public void SimpleRegistration()
    {
        var config = new NodeConfig(kernel =>
        {
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
            kernel.Register<TestNode5, DummyNodeNet<TestNode5>, DummyNodeRule<TestNodeRule5, TestNode5>>();
            kernel.Register<TestNode6, DummyNodeNet<TestNode6>, DummyNodeRule<TestNodeRule6, TestNode6>>();
            kernel.Register<TestNode7, DummyNodeNet<TestNode7>, DummyNodeRule<TestNodeRule7, TestNode7>>();
            kernel.Register<TestNode8, DummyNodeNet<TestNode8>, DummyNodeRule<TestNodeRule8, TestNode8>>();
        });
        
        var solver = new NodeSolver(config);
    }
    
    /// <summary>
    /// Registration of 2 solvers with completely different types.
    /// </summary>
    [Test]
    public void ParallelRegistration()
    {
        var config1 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
        });
        
        var config2 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
        });
        
        var solver1 = new NodeSolver(config1);
        var solver2 = new NodeSolver(config2);
    }
    
    /// <summary>
    /// Registration of 2 solvers with partially overlapping types.
    /// </summary>
    [Test]
    public void CrossRegistration()
    {
        var config1 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
        });
        
        var config2 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
        });
        
        var solver1 = new NodeSolver(config1);
        var solver2 = new NodeSolver(config2);
    }
    
    /// <summary>
    /// Registration of 4 solvers in a row with the same configuration.
    /// </summary>
    [Test]
    public void MassRegistration()
    {
        for (int i = 0; i < 4; i++)
        {
            var config = new NodeConfig(kernel =>
            {
                kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
                kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
                kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
                kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
            });
        
            var solver = new NodeSolver(config);
        }
    }
    
    /// <summary>
    /// Tests that attempting to register the same node type multiple times in a row throws an exception.
    /// </summary>
    [Test]
    public void BadRegistration()
    {
        var config = new NodeConfig(kernel =>
        {
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
        });

        Assert.Throws<ArgumentException>(() =>
        {
            var solver = new NodeSolver(config);
        });
    }
    
    /// <summary>
    /// Tests that the kernel can handle nodes being registered dynamically.
    /// </summary>
    [Test]
    public void DynamicRegistration()
    {
        var config = new NodeConfig(kernel =>
        {
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
        });

        var kernel = new NodeSolver(config);
        
        kernel.Register<TestNode5, DummyNodeNet<TestNode5>, DummyNodeRule<TestNodeRule5, TestNode5>>();
        kernel.Register<TestNode6, DummyNodeNet<TestNode6>, DummyNodeRule<TestNodeRule6, TestNode6>>();
        kernel.Register<TestNode7, DummyNodeNet<TestNode7>, DummyNodeRule<TestNodeRule7, TestNode7>>();
        kernel.Register<TestNode8, DummyNodeNet<TestNode8>, DummyNodeRule<TestNodeRule8, TestNode8>>();
    }

    private interface IDummyNodeRule;
    
    private struct TestNode1 : INode;
    private struct TestNode2 : INode;
    private struct TestNode3 : INode;
    private struct TestNode4 : INode;
    private struct TestNode5 : INode;
    private struct TestNode6 : INode;
    private struct TestNode7 : INode;
    private struct TestNode8 : INode;
    
    private struct TestNodeRule1 : IDummyNodeRule;
    private struct TestNodeRule2 : IDummyNodeRule;
    private struct TestNodeRule3 : IDummyNodeRule;
    private struct TestNodeRule4 : IDummyNodeRule;
    private struct TestNodeRule5 : IDummyNodeRule;
    private struct TestNodeRule6 : IDummyNodeRule;
    private struct TestNodeRule7 : IDummyNodeRule;
    private struct TestNodeRule8 : IDummyNodeRule;

    private struct DummyNodeNet<T>() : INodeNet<T, DummyNodeNet<T>>, INodeNetCreator<DummyNodeNet<T>> where T : INode
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

    private struct DummyNodeRule<T, TNode> : INodeRule<TNode>, INodeRuleCreator<DummyNodeRule<T, TNode>>
        where T : IDummyNodeRule
        where TNode : INode
    {
        public IEnumerable<NodeVoxel> Evaluate(INodeSolver solver, NodeVoxelHandle voxel, TNode node)
        {
            return [];
        }
        
        public static DummyNodeRule<T, TNode> CreateRule()
        {
            return new DummyNodeRule<T, TNode>();
        }
    }
}
