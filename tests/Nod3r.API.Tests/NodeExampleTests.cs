using Nod3r.Examples;
using Numos.Maths;

namespace Nod3r.API.Tests;

[TestFixture]
public sealed class NodeExampleTests
{
    private static readonly Int3[] SimpleNet = [
        new(0, 0, 0),
        new(1, 0, 0),
        new(2, 0, 0),
        new(0, 1, 0),
        new(0, 2, 0),
        new(1, 2, 0),
        new(2, 1, 0),
        new(2, 2, 0),
    ];
    
    /// <summary>
    /// Places a simple graph of nodes in a single chunk and checks
    /// that it properly makes a single network after the rebuild.
    /// </summary>
    [Test]
    public void SimpleNet_CreatesNetwork()
    {
        var world = new NodeWorld(reg =>
        {
            reg.Register<AdjacentNode, AdjacentNodeNet>();
        });
        
        var solver = world.CreateSimulation();
        var chunk = solver.EnsureChunk<AdjacentNode>(default);
        
        var node = new AdjacentNode();
        var refs = new List<VoxelRef>(SimpleNet.Length);
        foreach (var pos in SimpleNet)
        {
            refs.Add(solver.SetNode(node, chunk, pos));
        }
        
        solver.Rebuild();

        foreach (var pos in refs)
        {
            Assert.That(solver.HasNode<AdjacentNode>(pos), Is.True);
        }
        
        var nets = solver.GetNetworksSnapshot<AdjacentNodeNet>();
        Assert.That(nets, Has.Length.EqualTo(1));
        Assert.That(nets[0].Nodes, Has.Count.EqualTo(8));
    }

    private const int Dimensions = 16;
    
    /// <summary>
    /// Fills a chunk with adjacent nodes and checks that it makes a single network.
    /// Used for a benchmark.
    /// </summary>
    [Test]
    public void Fill_Rebuild_AdjacentNode()
    {
        var world = new NodeWorld(reg =>
        {
            reg.Register<AdjacentNode, AdjacentNodeNet>();
        });
        var simulation = world.CreateSimulation(Dimensions, Dimensions, Dimensions);
        var handle = simulation.EnsureChunk<AdjacentNode>(Int3Helpers.Zero);
        for (int z = 0; z < Dimensions; z++)
        {
            for (int y = 0; y < Dimensions; y++)
            {
                for (int x = 0; x < Dimensions; x++)
                {
                    simulation.SetNode(new AdjacentNode(), handle, new Int3(y, x, z));
                }
            }
        }
        
        simulation.Rebuild();
        
        var nets = simulation.GetNetworksSnapshot<AdjacentNodeNet>();
        Assert.That(nets, Has.Length.EqualTo(1));
        Assert.That(nets[0].Nodes, Has.Count.EqualTo(Dimensions *  Dimensions * Dimensions));
    }
}
