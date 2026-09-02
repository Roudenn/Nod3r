using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// A shared <see cref="GenIdStorage{T}"/> for every <see cref="INode"/> type for every layer.
/// </summary>
/// <typeparam name="T">Node type of this storage.</typeparam>
internal static class NodeStorage<T> where T : INode
{
    private readonly static Gen2DStorage<T> Storage = new();

    public static T Get(LayerId id)
    {
        return Storage[id];
    }
    
    public static T Get(ColumnHandle id, int layer)
    {
        return Storage[GetLayerId(id, layer)];
    }
    
    public static int GetFreeLayer(ColumnHandle id)
    {
        return Storage.GetFreeLayer(id);
    }

    public static LayerId GetLayerId(ColumnHandle idx, int layer)
    {
        return Storage.GetLayerId(idx, layer);
    }

    /// <summary>
    /// Allocates a new column and returns a reference to the target layer inside.
    /// </summary>
    public static void Add(T value, int layer, out LayerId id)
    {
        Storage.AddColumn(value, out id, layer);
    }
    
    /// <summary>
    /// Adds new space in a specific layer.
    /// </summary>
    public static void Add(T value, ColumnHandle idx, out LayerId id)
    {
        Storage.Add(value, idx, out id);
    }

    public static void Free(LayerId id)
    {
        Storage.Free(id);
    }
    
    public static void Free(ColumnHandle id, int layer)
    {
        Storage.Free(GetLayerId(id, layer));
    }

    public static void EnsureLayerCapacity(int capacity)
    {
        Storage.EnsureLayerCapacity(capacity);
    }
}
