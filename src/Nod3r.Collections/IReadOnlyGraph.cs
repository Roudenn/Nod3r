namespace Nod3r.Collections;

/// <summary>
/// Allows access to a read-only snapshot of a graph.
/// Only includes query operations.
/// </summary>
/// <typeparam name="T">Type of the nodes in the graph.</typeparam>
public interface IReadOnlyGraph<T>
{
    Span<GenId> GetConnections(GenId id);

    void CalculateDepthMap(GenId start);
}
