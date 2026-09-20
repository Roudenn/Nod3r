using Nod3r.API;
using Nod3r.Types;

namespace Nod3r.Solver.Tests;

/// <summary>
/// Tests that chunks are initialized properly both for type and ID nodes.
/// </summary>
[TestFixture]
public class ChunkInitializationTest
{
    /// <summary>
    /// Tests that a chunk can be properly added to the simulation.
    /// </summary>
    [Test]
    public void TypeChunkTest()
    {
        var config = new NodeConfig(kernel =>
        {
            kernel.Register<TestNode1, DummyNodeNet<TestNode1>, DummyNodeRule<TestNode1>>();
        });
        
        var solver = new NodeSolver(config);
        solver.EnsureChunk<TestNode1>(Int3Helpers.Zero);
        var snapshot = solver.GetChunksSnapshot<TestNode1>();
        Assert.That(snapshot, Has.Length.EqualTo(1));
    }
}
