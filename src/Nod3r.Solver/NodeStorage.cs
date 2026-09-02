using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// A shared <see cref="GenIdStorage{T}"/> for every <see cref="INode"/> type for every layer.
/// </summary>
/// <typeparam name="T">Node type of this storage.</typeparam>
internal static class NodeStorage<T> where T : INode
{
    private static Gen2DStorage<T> _storage = new();

    public static ref T Get(LayerId id)
    {
        return ref _storage[id];
    }
    
    public static ref T Get(ColumnHandle id, int layer)
    {
        return ref _storage[GetLayerId(id, layer)];
    }
    
    public static int GetFreeLayer(ColumnHandle id)
    {
        return _storage.GetFreeLayer(id);
    }

    public static LayerId GetLayerId(ColumnHandle idx, int layer)
    {
        return _storage.GetLayerId(idx, layer);
    }

    /// <summary>
    /// Allocates a new column and returns a reference to the target layer inside.
    /// </summary>
    public static ref T Allocate(int layer, out LayerId id)
    {
        return ref _storage.AllocateColumn(out id, layer);
    }
    
    /// <summary>
    /// Allocates new space in a specific layer.
    /// </summary>
    public static ref T Allocate(ColumnHandle idx, out LayerId id)
    {
        return ref _storage.Allocate(idx, out id);
    }

    public static void Free(LayerId id)
    {
        _storage.Free(id);
    }
    
    public static void Free(ColumnHandle id, int layer)
    {
        _storage.Free(GetLayerId(id, layer));
    }

    public static void EnsureLayerCapacity(int capacity)
    {

    }
}
