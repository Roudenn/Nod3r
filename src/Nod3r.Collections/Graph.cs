using System.Diagnostics.CodeAnalysis;

namespace Nod3r.Collections;

/// <summary>
/// Represents a graph of objects where each element is stored
/// as a node that can connect to other nodes.
/// </summary>
public class Graph<T> : GenIdStorage<T>, IReadOnlyGraph<T>
{
    /// <summary>
    /// All connection of every index.
    /// </summary>
    private GenId[][] _connections;
    
    /// <summary>
    /// Default size of the connections array.
    /// </summary>
    public int ConnectionCount { get; private set; }
    
    /// <summary>
    /// A dummy array that is returned if no valid connections are present.
    /// </summary>
    private readonly GenId[] _dummyArray = [];
    
    public Graph(int capacity = 16, int connectCount = 6)
    {
        ConnectionCount = connectCount;
        _connections = new GenId[capacity][];
        for (int i = 0; i < capacity; i++)
        {
            _connections[i] = new GenId[connectCount];
        }
    }

    /// <summary>
    /// Returns all connections from a specific index.
    /// </summary>
    public GenId[] GetConnections(GenId index)
    {
        return !IsValid(index) ? _dummyArray : _connections[index.Index];
    }

    /// <summary>
    /// Adds a value to the graph and returns the assigned <see cref="GenId"/> and
    /// array of connections to other nodes that must be filled after this method.
    /// </summary>
    /// <param name="value">The data to add into the graph as a node.</param>
    /// <param name="id"><see cref="GenId"/> reference to the data node.</param>
    /// <param name="connections">An array of connections that must be filled with connections later.</param>
    public void Add(T value, out GenId id, out GenId[] connections)
    {
        base.Add(value, out id);
        connections = _connections[id.Index];
    }

    public override void Free(GenId id)
    {
        base.Free(id);
        Array.Fill(_connections[id.Index], GenId.Invalid); // Cut all connections
    }

    protected override void ReAllocateTo(int newSize)
    {
        int oldLength = Length;
        base.ReAllocateTo(newSize);
        
        Array.Resize(ref _connections, newSize);
        for (int i = oldLength; i < ConnectionCount; i++)
        {
            _connections[i] = new GenId[ConnectionCount];
        }
    }

    /// <summary>
    /// Enumerates through the graph using Breadth-First Search (BFS).
    /// </summary>
    public struct BFSEnumerator : IGraphEnumerator<T>
    {
        /// <summary>
        /// The target graph.
        /// </summary>
        private readonly Graph<T> _graph;
        
        /// <summary>
        /// Queue of indexes to process on the next iteration.
        /// </summary>
        private readonly Queue<GenId> _indexQueue = new();
        
        private readonly HashSet<int> _traversed = new();
        
        /// <summary>
        /// Last processed index.
        /// </summary>
        public GenId LastIndex { get; private set; }

        /// <summary>
        /// Current element of the enumerator.
        /// </summary>
        private T _current;

        public BFSEnumerator(Graph<T> graph, GenId startIndex)
        {
            _indexQueue.Enqueue(startIndex);
            _graph = graph;
            _current = default!;
        }
        
        public bool MoveNext([NotNullWhen(true)] out T? value)
        {
            value = default;
            
            if (!_indexQueue.TryDequeue(out var index)
                || _traversed.Contains(index.Index))
                return false;
            
            var connections = _graph.GetConnections(index);
            foreach (var genId in connections)
            {
                if (genId.IsValid)
                    _indexQueue.Enqueue(genId);
            }
            
            LastIndex = index;
            _current = _graph[index];
            value = _current!;
            _traversed.Add(index.Index);
            return true;
        }
    }
    
    /// <summary>
    /// Enumerates through the graph using Depth-First Search (DFS).
    /// </summary>
    /// <remarks>
    /// Depth-First Search tries to go into deep branches first, before going to other branches.
    /// This usually
    /// </remarks>
    public struct DFSEnumerator : IGraphEnumerator<T>
    {
        /// <summary>
        /// The target graph.
        /// </summary>
        private readonly Graph<T> _graph;
        
        /// <summary>
        /// Queue of indexes to process on the next iteration.
        /// </summary>
        private readonly Stack<GenId> _indexStack = new();
        
        private readonly HashSet<int> _traversed = new();
        
        /// <summary>
        /// Last processed index.
        /// </summary>
        public GenId LastIndex { get; private set; }

        /// <summary>
        /// Current element of the enumerator.
        /// </summary>
        private T _current;
        
        public DFSEnumerator(Graph<T> graph, GenId startIndex)
        {
            _indexStack.Push(startIndex);
            _graph = graph;
            _current = default!;
        }
        
        public bool MoveNext([NotNullWhen(true)] out T? value)
        {
            value = default;
            
            if (!_indexStack.TryPop(out var index)
                || _traversed.Contains(index.Index))
                return false;

            var connections = _graph.GetConnections(index);
            foreach (var genId in connections)
            {
                if (genId.IsValid)
                    _indexStack.Push(genId);
            }
            
            LastIndex = index;
            _current = _graph[index];
            value = _current!;
            _traversed.Add(index.Index);
            return true;
        }
    }

    public IGraphEnumerator<T> GetBFSEnumerator(GenId start)
    {
        return new BFSEnumerator(this, start);
    }

    public IGraphEnumerator<T> GetDFSEnumerator(GenId start)
    {
        return new DFSEnumerator(this, start);
    }
}
