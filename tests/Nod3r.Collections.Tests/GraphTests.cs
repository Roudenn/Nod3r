namespace Nod3r.Collections.Tests;

[TestFixture]
public sealed class GraphTests
{
    public void Add_Graph_Test()
    {
        var graph = new Graph<int>();
        var node1 = graph.AddNode(0);
    }
}
