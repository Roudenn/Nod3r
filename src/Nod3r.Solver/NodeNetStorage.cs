using Nod3r.Collections;

namespace Nod3r.Solver;

internal abstract class NodeNetStorage;

/// <summary>
/// Kernel-specific storage for all <see cref="INodeNet"/> types.
/// </summary>
/// <typeparam name="T">Type of node net.</typeparam>
internal sealed class NodeNetStorage<T> : NodeNetStorage where T : INodeNet
{
    private readonly GenIdStorage<T> _storage = new();
    
    public GenId Add(T network)
    {
        return _storage.Add(network);
    }

    public T Get(GenId id)
    {
        return _storage[id];
    }
    
    public ref T GetRef(GenId id)
    {
        return ref _storage[id];
    }

    public void Free(GenId id)
    {
        _storage.Free(id);
    }
}
