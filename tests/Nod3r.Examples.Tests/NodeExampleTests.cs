using Nod3r.API;
using Nod3r.Collections;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Examples.Tests;

[TestFixture]
public sealed class NodeExampleTests
{
    [Test]
    public void TestAdjacentNode()
    {
        var config = new NodeConfig(reg =>
        {
            reg.Register<AdjacentNode, AdjacentNodeNet, AdjacentNodeRule<AdjacentNode>>();
        });
        var solver = new NodeSolver(config);

        var chunk = solver.EnsureChunk<AdjacentNode>(default);
        
        var node = new AdjacentNode();
        
        solver.SetNode(node, chunk, new Int3(0, 0, 0), 0);
        solver.SetNode(node, chunk, new Int3(1, 0, 0), 0);
        solver.SetNode(node, chunk, new Int3(2, 0, 0), 0);
        solver.SetNode(node, chunk, new Int3(0, 1, 0), 0);
        solver.SetNode(node, chunk, new Int3(0, 2, 0), 0);
        solver.SetNode(node, chunk, new Int3(1, 2, 0), 0);
        solver.SetNode(node, chunk, new Int3(2, 1, 0), 0);
        solver.SetNode(node, chunk, new Int3(2, 2, 0), 0);
        
        solver.Rebuild();

        var nets = solver.GetNetworksSnapshot<AdjacentNodeNet>();
        Assert.That(nets, Has.Count.EqualTo(1));
    }
    
    private record struct AdjacentNode() : INode
    {
        public float Capacity = 1f;
    }
    
    private struct AdjacentNodeNet() : INodeNet<AdjacentNode, AdjacentNodeNet>, INodeNetCreator<AdjacentNodeNet>
    {
        public float TotalCapacity = 0f;

        public HashSet<LayerId> Nodes { get; } = new();

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        public void Merge(IReadOnlySet<AdjacentNodeNet> nets)
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
