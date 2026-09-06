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
            kernel.Register<TestNode1, DummyNodeNet<TestNodeNet1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNodeNet2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNodeNet3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNodeNet4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
            kernel.Register<TestNode5, DummyNodeNet<TestNodeNet5>, DummyNodeRule<TestNodeRule5, TestNode5>>();
            kernel.Register<TestNode6, DummyNodeNet<TestNodeNet6>, DummyNodeRule<TestNodeRule6, TestNode6>>();
            kernel.Register<TestNode7, DummyNodeNet<TestNodeNet7>, DummyNodeRule<TestNodeRule7, TestNode7>>();
            kernel.Register<TestNode8, DummyNodeNet<TestNodeNet8>, DummyNodeRule<TestNodeRule8, TestNode8>>();
        });
        
        var solver = new NodeKernel(config);
    }
    
    /// <summary>
    /// Registration of 2 solvers with completely different types.
    /// </summary>
    [Test]
    public void ParallelRegistration()
    {
        var config1 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode1, DummyNodeNet<TestNodeNet1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNodeNet2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
        });
        
        var config2 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode3, DummyNodeNet<TestNodeNet3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNodeNet4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
        });
        
        var solver1 = new NodeKernel(config1);
        var solver2 = new NodeKernel(config2);
    }
    
    /// <summary>
    /// Registration of 2 solvers with partially overlapping types.
    /// </summary>
    [Test]
    public void CrossRegistration()
    {
        var config1 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode1, DummyNodeNet<TestNodeNet1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNodeNet2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNodeNet3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
        });
        
        var config2 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode2, DummyNodeNet<TestNodeNet2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNodeNet3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNodeNet4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
        });
        
        var solver1 = new NodeKernel(config1);
        var solver2 = new NodeKernel(config2);
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
                kernel.Register<TestNode1, DummyNodeNet<TestNodeNet1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
                kernel.Register<TestNode2, DummyNodeNet<TestNodeNet2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
                kernel.Register<TestNode3, DummyNodeNet<TestNodeNet3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
                kernel.Register<TestNode4, DummyNodeNet<TestNodeNet4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
            });
        
            var solver = new NodeKernel(config);
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
            kernel.Register<TestNode1, DummyNodeNet<TestNodeNet1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNodeNet2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode1, DummyNodeNet<TestNodeNet1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
        });

        Assert.Throws<ArgumentException>(() =>
        {
            var solver = new NodeKernel(config);
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
            kernel.Register<TestNode1, DummyNodeNet<TestNodeNet1>, DummyNodeRule<TestNodeRule1, TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNodeNet2>, DummyNodeRule<TestNodeRule2, TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNodeNet3>, DummyNodeRule<TestNodeRule3, TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNodeNet4>, DummyNodeRule<TestNodeRule4, TestNode4>>();
        });

        var kernel = new NodeKernel(config);
        
        kernel.Register<TestNode5, DummyNodeNet<TestNodeNet5>, DummyNodeRule<TestNodeRule5, TestNode5>>();
        kernel.Register<TestNode6, DummyNodeNet<TestNodeNet6>, DummyNodeRule<TestNodeRule6, TestNode6>>();
        kernel.Register<TestNode7, DummyNodeNet<TestNodeNet7>, DummyNodeRule<TestNodeRule7, TestNode7>>();
        kernel.Register<TestNode8, DummyNodeNet<TestNodeNet8>, DummyNodeRule<TestNodeRule8, TestNode8>>();
    }

    private interface IDummyNodeNet;
    private interface IDummyNodeRule;
    
    private struct TestNode1 : INode;
    private struct TestNode2 : INode;
    private struct TestNode3 : INode;
    private struct TestNode4 : INode;
    private struct TestNode5 : INode;
    private struct TestNode6 : INode;
    private struct TestNode7 : INode;
    private struct TestNode8 : INode;
    
    private struct TestNodeNet1 : IDummyNodeNet;
    private struct TestNodeNet2 : IDummyNodeNet;
    private struct TestNodeNet3 : IDummyNodeNet;
    private struct TestNodeNet4 : IDummyNodeNet;
    private struct TestNodeNet5 : IDummyNodeNet;
    private struct TestNodeNet6 : IDummyNodeNet;
    private struct TestNodeNet7 : IDummyNodeNet;
    private struct TestNodeNet8 : IDummyNodeNet;
    
    private struct TestNodeRule1 : IDummyNodeRule;
    private struct TestNodeRule2 : IDummyNodeRule;
    private struct TestNodeRule3 : IDummyNodeRule;
    private struct TestNodeRule4 : IDummyNodeRule;
    private struct TestNodeRule5 : IDummyNodeRule;
    private struct TestNodeRule6 : IDummyNodeRule;
    private struct TestNodeRule7 : IDummyNodeRule;
    private struct TestNodeRule8 : IDummyNodeRule;

    private struct DummyNodeNet<T> : INodeNet, INodeNetCreator<DummyNodeNet<T>> where T : IDummyNodeNet
    {
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

        public static DummyNodeNet<T> CreateNet(INodeNetInternal net)
        {
            return new DummyNodeNet<T>();
        }
    }

    private struct DummyNodeRule<T, TNode> : INodeRule<TNode>, INodeRuleCreator<DummyNodeRule<T, TNode>>
        where T : IDummyNodeRule
        where TNode : INode
    {
        public IEnumerable<NodeVoxel> Evaluate(INodeKernel solver, NodeVoxel voxel, TNode node)
        {
            return [];
        }
        
        public static DummyNodeRule<T, TNode> CreateRule()
        {
            return new DummyNodeRule<T, TNode>();
        }
    }
}
