using Nod3r.API;

namespace Nod3r.Solver.Tests;

/// <summary>
/// Tests that chunks are initialized properly both for type and ID nodes.
/// </summary>
[TestFixture]
public class ChunkInitializationTest
{
    /// <summary>
    /// Tests that a type node chunk can be properly added to the simulation.
    /// </summary>
    [Test]
    public void TypeChunkTest()
    {
        var config = new NodeConfig(kernel =>
        {
            kernel.RegisterTest<TestNode1, DummyNodeNet<TestNode1>>();
        });
        
        var world = new NodeWorld(config);
        var solver = world.CreateSimulation();
        solver.EnsureChunk<TestNode1>(Int3Helpers.Zero);
        var snapshot = solver.GetChunksSnapshot<TestNode1>();
        Assert.That(snapshot, Has.Length.EqualTo(1));
    }
    
    /// <summary>
    /// Tests that an ID node chunk can be properly added to the simulation.
    /// </summary>
    [Test]
    public void IDChunkTest()
    {
        var config = new NodeConfig(kernel =>
        {
            kernel.RegisterID<DummyNodeNet<TestNode1>>(NodeRuleBuilder.Empty, 0);
        });
        
        var world = new NodeWorld(config);
        var solver = world.CreateSimulation();
        solver.EnsureChunk(0, Int3Helpers.Zero);
        var snapshot = solver.GetChunksSnapshot(0);
        Assert.That(snapshot, Has.Length.EqualTo(1));
    }
}
