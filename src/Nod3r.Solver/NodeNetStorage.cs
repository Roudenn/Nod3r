using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// Shared storage for all <see cref="INodeNet"/> types.
/// </summary>
/// <typeparam name="T">Type of node net.</typeparam>
public static class NodeNetStorage<T> where T : INodeNet
{
    private static readonly GenIdStorage<T> Storage = new();
    
    public static GenId Add(T network)
    {
        return Storage.Add(network);
    }

    public static T Get(GenId id)
    {
        return Storage[id];
    }
}
