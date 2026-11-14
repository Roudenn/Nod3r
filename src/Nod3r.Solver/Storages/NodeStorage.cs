using Nod3r.Collections;

namespace Nod3r.Solver.Storages;

/// <summary>
/// A shared <see cref="GenIdStorage{T}"/> for every <see cref="INode"/> type for every layer.
/// </summary>
/// <typeparam name="T">Node type of this storage.</typeparam>
internal sealed class NodeStorage<T> where T : INode
{
    private readonly Gen2DStorage<NodeData<T>> _storage = new();

    public int LayerCapacity => _storage.LayerCapacity;
    
    public T GetData(LayerId id)
    {
        return _storage[id].Data;
    }
    
    public T GetData(ColumnHandle id, int layer)
    {
        return _storage[GetLayerId(id, layer)].Data;
    }
    
    public NodeData<T> Get(LayerId id)
    {
        return _storage[id];
    }
    
    public NodeData<T> Get(ColumnHandle id, int layer)
    {
        return _storage[GetLayerId(id, layer)];
    }
    
    public byte GetFreeLayer(ColumnHandle id)
    {
        return (byte) _storage.GetFreeLayer(id);
    }

    public LayerId GetLayerId(ColumnHandle idx, int layer)
    {
        return _storage.GetLayerId(idx, layer);
    }

    /// <summary>
    /// Allocates a new column and returns a reference to the target layer inside.
    /// </summary>
    public void Add(T value, int layer, out LayerId id, GenId? netId = null)
    {
        _storage.AddColumn(new NodeData<T>(value, netId ?? GenId.Invalid), out id, layer);
    }
    
    /// <summary>
    /// Adds new space in a specific layer.
    /// </summary>
    public void Add(T value, ColumnHandle idx, out LayerId id, GenId? netId = null)
    {
        _storage.Add(new NodeData<T>(value, netId ?? GenId.Invalid), idx, out id);
    }
    
    public void Set(LayerId id, T value)
    {
        if (!_storage.TryGetValue(id, out var data))
            data = new NodeData<T>(value, GenId.Invalid);
        
        _storage.Set(id, data);
    }
    
    public void Set(LayerId id, NodeData<T> value)
    {
        _storage.Set(id, value);
    }

    public void Free(LayerId id)
    {
        _storage.Free(id);
    }
    
    public void Free(ColumnHandle id, int layer)
    {
        _storage.Free(GetLayerId(id, layer));
    }

    public void EnsureLayerCapacity(int capacity)
    {
        _storage.EnsureLayerCapacity(capacity);
    }

    public void GetColumnData(ColumnHandle idx, List<NodeData<T>> list)
    {
        _storage.GetColumnData(idx, list);
    }
}

/// <summary>
/// 
/// </summary>
/// <param name="Data"></param>
/// <param name="NetId"></param>
/// <typeparam name="T"></typeparam>
public record struct NodeData<T>(T Data, GenId NetId) where T : INode;
