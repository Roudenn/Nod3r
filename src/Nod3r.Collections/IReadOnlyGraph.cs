using System.Diagnostics.CodeAnalysis;

namespace Nod3r.Collections;

/// <summary>
/// Interface wrapper that allows to iterate through a graph.
/// </summary>
public interface IReadOnlyGraph<T>
{
    /// <summary>
    /// Gets the readonly Breadth-First Search (BFS) enumerator for this graph.
    /// </summary>
    IGraphEnumerator<T> GetBFSEnumerator(GenId start);
    
    /// <summary>
    /// Gets the readonly Depth-First Search (DFS) enumerator for this graph.
    /// </summary>
    IGraphEnumerator<T> GetDFSEnumerator(GenId start);
}

public interface IGraphEnumerator<T>
{
    bool MoveNext([NotNullWhen(true)] out T? value);
}
