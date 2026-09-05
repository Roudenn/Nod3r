using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

internal abstract class NodeStorage
{
    public abstract int GetFreeLayer(ColumnHandle id);

    public abstract LayerId GetLayerId(ColumnHandle idx, int layer);

    public abstract void Free(LayerId id);

    public abstract void Free(ColumnHandle id, int layer);

    public abstract void EnsureLayerCapacity(int capacity);
}

/// <summary>
/// A shared <see cref="GenIdStorage{T}"/> for every <see cref="INode"/> type for every layer.
/// </summary>
/// <typeparam name="T">Node type of this storage.</typeparam>
internal sealed class NodeStorage<T> : NodeStorage where T : INode
{
    private readonly Gen2DStorage<T> _storage = new();

    public T Get(LayerId id)
    {
        return _storage[id];
    }
    
    public T Get(ColumnHandle id, int layer)
    {
        return _storage[GetLayerId(id, layer)];
    }
    
    public override int GetFreeLayer(ColumnHandle id)
    {
        return _storage.GetFreeLayer(id);
    }

    public override LayerId GetLayerId(ColumnHandle idx, int layer)
    {
        return _storage.GetLayerId(idx, layer);
    }

    /// <summary>
    /// Allocates a new column and returns a reference to the target layer inside.
    /// </summary>
    public void Add(T value, int layer, out LayerId id)
    {
        _storage.AddColumn(value, out id, layer);
    }
    
    /// <summary>
    /// Adds new space in a specific layer.
    /// </summary>
    public void Add(T value, ColumnHandle idx, out LayerId id)
    {
        _storage.Add(value, idx, out id);
    }

    public override void Free(LayerId id)
    {
        _storage.Free(id);
    }
    
    public override void Free(ColumnHandle id, int layer)
    {
        _storage.Free(GetLayerId(id, layer));
    }

    public override void EnsureLayerCapacity(int capacity)
    {
        _storage.EnsureLayerCapacity(capacity);
    }
}
