using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

internal abstract class NodeNetStorage
{
    public abstract void Free(GenId id);
}

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

    public override void Free(GenId id)
    {
        _storage.Free(id);
    }
}
