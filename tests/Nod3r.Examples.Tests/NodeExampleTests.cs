using Nod3r.API;
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
            reg.Register<AdjacentNode, AdjacentNodeNet, AdjacentNodeRule<AdjacentNode>>(new());
        });
        var solver = new NodeSolver(config);

        var chunk = solver.EnsureChunk(default);
        
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
    }
}
