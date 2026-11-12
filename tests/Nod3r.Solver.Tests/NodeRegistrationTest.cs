using Nod3r.API;
using Nod3r.API.Rules;

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
        var solver = new NodeWorld(kernel =>
        {
            kernel.RegisterTest<TestNode1, DummyNodeNet<TestNode1>>();
            kernel.RegisterTest<TestNode2, DummyNodeNet<TestNode2>>();
            kernel.RegisterTest<TestNode3, DummyNodeNet<TestNode3>>();
            kernel.RegisterTest<TestNode4, DummyNodeNet<TestNode4>>();
            kernel.RegisterTest<TestNode5, DummyNodeNet<TestNode5>>();
            kernel.RegisterTest<TestNode6, DummyNodeNet<TestNode6>>();
            kernel.RegisterTest<TestNode7, DummyNodeNet<TestNode7>>();
            kernel.RegisterTest<TestNode8, DummyNodeNet<TestNode8>>();
            
            kernel.RegisterID<DummyNodeNet<TestNode8>>(NodeRuleBuilder.Create().Compile(), 0);
        });
    }
    
    /// <summary>
    /// Registration of 2 solvers with completely different types.
    /// </summary>
    [Test]
    public void ParallelRegistration()
    {
        var solver1 = new NodeWorld(kernel =>
        {
            kernel.RegisterTest<TestNode1, DummyNodeNet<TestNode1>>();
            kernel.RegisterTest<TestNode2, DummyNodeNet<TestNode2>>();
        });
        var solver2 = new NodeWorld(kernel =>
        {
            kernel.RegisterTest<TestNode3, DummyNodeNet<TestNode3>>();
            kernel.RegisterTest<TestNode4, DummyNodeNet<TestNode4>>();
        });
    }
    
    /// <summary>
    /// Registration of 2 solvers with partially overlapping types.
    /// </summary>
    [Test]
    public void CrossRegistration()
    {
        var solver1 = new NodeWorld(kernel =>
        {
            kernel.RegisterTest<TestNode1, DummyNodeNet<TestNode1>>();
            kernel.RegisterTest<TestNode2, DummyNodeNet<TestNode2>>();
            kernel.RegisterTest<TestNode3, DummyNodeNet<TestNode3>>();
        });
        var solver2 = new NodeWorld(kernel =>
        {
            kernel.RegisterTest<TestNode2, DummyNodeNet<TestNode2>>();
            kernel.RegisterTest<TestNode3, DummyNodeNet<TestNode3>>();
            kernel.RegisterTest<TestNode4, DummyNodeNet<TestNode4>>();
        });
    }
    
    /// <summary>
    /// Registration of 4 solvers in a row with the same configuration.
    /// </summary>
    [Test]
    public void MassRegistration()
    {
        for (int i = 0; i < 4; i++)
        {
            var solver = new NodeWorld(kernel =>
            {
                kernel.RegisterTest<TestNode1, DummyNodeNet<TestNode1>>();
                kernel.RegisterTest<TestNode2, DummyNodeNet<TestNode2>>();
                kernel.RegisterTest<TestNode3, DummyNodeNet<TestNode3>>();
                kernel.RegisterTest<TestNode4, DummyNodeNet<TestNode4>>();
            });
        }
    }
    
    /// <summary>
    /// Tests that attempting to register the same node type multiple times in a row throws an exception.
    /// </summary>
    [Test]
    public void BadRegistration()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var solver = new NodeWorld(kernel =>
            {
                kernel.RegisterTest<TestNode1, DummyNodeNet<TestNode1>>();
                kernel.RegisterTest<TestNode2, DummyNodeNet<TestNode2>>();
                kernel.RegisterTest<TestNode1, DummyNodeNet<TestNode1>>();
            });
        });
    }
    
    /// <summary>
    /// Tests that the kernel can handle nodes being registered dynamically.
    /// </summary>
    [Test]
    public void DynamicRegistration()
    {
        var kernel = new NodeWorld(kernel =>
        {
            kernel.RegisterTest<TestNode1, DummyNodeNet<TestNode1>>();
            kernel.RegisterTest<TestNode2, DummyNodeNet<TestNode2>>();
            kernel.RegisterTest<TestNode3, DummyNodeNet<TestNode3>>();
            kernel.RegisterTest<TestNode4, DummyNodeNet<TestNode4>>();
        });
        
        kernel.RegisterTest<TestNode5, DummyNodeNet<TestNode5>>();
        kernel.RegisterTest<TestNode6, DummyNodeNet<TestNode6>>();
        kernel.RegisterTest<TestNode7, DummyNodeNet<TestNode7>>();
        kernel.RegisterTest<TestNode8, DummyNodeNet<TestNode8>>();
    }
}
