using Nod3r.API.Registration;
using Nod3r.API.Rules;
using Nod3r.Collections;
using Nod3r.Solver;
using Nod3r.Solver.Factories;
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
    }
    
    private record struct AdjacentNode() : INode, INodeAutoRegistration<AdjacentNode>
    {
        public float Capacity = 1f;
        
        public static NodeRule<AdjacentNode> ConnectionRule =>
            NodeRuleBuilder.Create()
            .IncludeCartesian()
            .Compile<AdjacentNode>();
    }
    
    private struct AdjacentNodeNet() : INodeNet<AdjacentNode, AdjacentNodeNet>, INodeNetCreator<AdjacentNodeNet>
    {
        public float TotalCapacity = 0f;

        public GenId Id { get; }
        
        public HashSet<LayerId> Nodes { get; } = new();

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        public void Merge(IReadOnlyList<AdjacentNodeNet> nets)
        {
        }

        public void Split(AdjacentNodeNet parent)
        {
        }

        public static AdjacentNodeNet CreateNet()
        {
            return new AdjacentNodeNet();
        }
    }
}
