namespace Nod3r.Collections.Tests;

[TestFixture]
public sealed class GraphTests
{
    [Test]
    public void TestBFSEnumerator()
    {
        var graph = new Graph<int>();
        graph.Add(1, out var first, out var connections1);
        graph.Add(2, out var second, out var connections2);
        graph.Add(3, out var third, out var connections3);
        graph.Add(4, out var fourth, out var connections4);

        connections1[0] = second;
        connections1[1] = third;
        connections2[0] = fourth;
        connections3[0] = fourth;

        int j = 0;
        var query = graph.GetBFSEnumerator(first);
        while (query.MoveNext(out var i))
        {
            j++;
            Assert.That(i, Is.EqualTo(j));
        }
    }
    
    [Test]
    public void TestDFSEnumerator()
    {
        var graph = new Graph<int>();
        graph.Add(1, out var first, out var connections1);
        graph.Add(2, out var second, out var connections2);
        graph.Add(3, out var third, out var connections3);
        graph.Add(4, out var fourth, out var connections4);

        connections1[0] = third;
        connections1[1] = second;
        connections2[0] = fourth;
        connections3[0] = fourth;

        int j = 0;
        var query = graph.GetDFSEnumerator(first);
        while (query.MoveNext(out var i))
        {
            j++;

            switch (j)
            {
                case 3:
                    Assert.That(i, Is.EqualTo(4));
                    break;
                case 4:
                    Assert.That(i, Is.EqualTo(3));
                    break;
            }
        }
    }
}
