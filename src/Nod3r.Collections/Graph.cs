namespace Nod3r.Collections;

/// <summary>
/// A dynamic graph that implements methods to add, remove, and query through its elements.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Graph<T>(int capacity = 16) : IReadOnlyGraph<T>
{
    /// <summary>
    /// The internal storage that stores all nodes of the graph.
    /// </summary>
    private readonly GenIdStorage<Edge> _storage = new(capacity);
    
    /// <summary>
    /// Edges from every node to their connected nodes in the internal data storage.
    /// </summary>
    private readonly FixedSpaceArray<GenId> _edges = new(capacity);

    /// <summary>
    /// Adds a new node of type <see cref="T"/> to the graph and connects to other specified nodes.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="connections"></param>
    /// <returns><see cref="GenId"/> of the newly added node.</returns>
    public GenId AddNode(T data, params ReadOnlySpan<GenId> connections)
    {
        var span = _edges.Allocate(connections.Length, out var genId);
        connections.CopyTo(span.Span);
        _storage.Add(new Edge(data, genId), out var id);
        return id;
    }
    
    /// <summary>
    /// Removes the node from the graph, invalidating all
    /// connections pointing to it and marking the allocated space as free.
    /// </summary>
    public void RemoveNode(GenId id)
    {
        if (!_storage.TryGetValue(id, out var edge))
            return;
        
        _edges.Free(edge.VerticesId);
        _storage.Free(id);
    }

    /// <summary>
    /// Ensures that the graph has enough capacity
    /// for at least the specified amount of nodes.
    /// </summary>
    /// <param name="capacity"></param>
    public void EnsureCapacity(int capacity)
    {
        _storage.EnsureCapacity(capacity);
        _edges.EnsureCapacity(capacity);
    }

    public Span<GenId> GetConnections(GenId id)
    {
        return !_storage.TryGetValue(id, out var edge) ? Span<GenId>.Empty : _edges.GetSpan(edge.VerticesId);
    }

    public void CalculateDepthMap(GenId start)
    {
        var curNodes = new Stack<GenId>();
        curNodes.Push(start);
        int depth = 0;
        while (curNodes.Count > 0)
        {
            var curNode = curNodes.Pop();
            var curVertex = _storage[curNode].VerticesId;
            _storage[curNode].Depth = depth;
            var vertices = GetConnections(curVertex);
            foreach (var vertex in vertices)
            {
                curNodes.Push(vertex);
            }

            depth++;
        }
    }

    // TODO: SoA
    private record struct Edge(T Data, GenId VerticesId, int Depth = 0);
}
