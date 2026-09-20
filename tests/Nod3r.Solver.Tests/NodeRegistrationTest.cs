using Nod3r.API;
using Nod3r.Types;

namespace Nod3r.Solver.Tests;

/// <summary>
/// Test for different node type registration scenarios.
/// </summary>
[TestFixture]
public sealed class NodeRegistrationTest
{
    /// <summary>
    /// Simple registration of a single solver instance with multiple node types.
    /// </summary>
    [Test]
    public void SimpleRegistration()
    {
        var config = new NodeConfig(kernel =>
        {
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNode4>>();
            kernel.Register<TestNode5, DummyNodeNet<TestNode5>, DummyNodeRule<TestNode5>>();
            kernel.Register<TestNode6, DummyNodeNet<TestNode6>, DummyNodeRule<TestNode6>>();
            kernel.Register<TestNode7, DummyNodeNet<TestNode7>, DummyNodeRule<TestNode7>>();
            kernel.Register<TestNode8, DummyNodeNet<TestNode8>, DummyNodeRule<TestNode8>>();
            
            kernel.RegisterID<DummyNodeNet<TestNode8>, DummyNodeRuleID>(0, out _);
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
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNode2>>();
        });
        
        var config2 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNode4>>();
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
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNode3>>();
        });
        
        var config2 = new NodeConfig((kernel) =>
        {
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNode4>>();
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
                kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNode1>>();
                kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNode2>>();
                kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNode3>>();
                kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNode4>>();
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
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNode2>>();
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNode1>>();
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
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNode1>>();
            kernel.Register<TestNode2, DummyNodeNet<TestNode2>, DummyNodeRule<TestNode2>>();
            kernel.Register<TestNode3, DummyNodeNet<TestNode3>, DummyNodeRule<TestNode3>>();
            kernel.Register<TestNode4, DummyNodeNet<TestNode4>, DummyNodeRule<TestNode4>>();
        });

        var kernel = new NodeSolver(config);
        
        kernel.Register<TestNode5, DummyNodeNet<TestNode5>, DummyNodeRule<TestNode5>>();
        kernel.Register<TestNode6, DummyNodeNet<TestNode6>, DummyNodeRule<TestNode6>>();
        kernel.Register<TestNode7, DummyNodeNet<TestNode7>, DummyNodeRule<TestNode7>>();
        kernel.Register<TestNode8, DummyNodeNet<TestNode8>, DummyNodeRule<TestNode8>>();
    }
}
