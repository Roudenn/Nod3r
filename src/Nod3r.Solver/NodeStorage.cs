using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// A shared <see cref="GenIdStorage{T}"/> for every <see cref="INode"/> type for every layer.
/// </summary>
/// <typeparam name="T">Node type of this storage.</typeparam>
internal static class NodeStorage<T> where T : INode
{
    private static StackIdStorage<T> _storage = new();

    public static ref T Get(StackGenId id)
    {
        return ref _storage[id];
    }
    
    public static ref T Get(GenIdx id, int layer)
    {
        return ref _storage[id, layer];
    }

    public static StackGenId GetStackId(GenIdx idx, int layer)
    {
        return _storage.GetStackId(idx, layer);
    }

    /// <summary>
    /// Allocates a new column and returns a reference to the target layer inside.
    /// </summary>
    public static ref T Allocate(int layer, out StackGenId id)
    {
        return ref _storage.AllocateColumn(out id, layer);
    }
    
    /// <summary>
    /// Allocates new space in a specific layer.
    /// </summary>
    public static ref T Allocate(GenIdx idx, out StackGenId id)
    {
        return ref _storage.Allocate(idx, out id);
    }

    public static void Free(StackGenId id)
    {
        _storage.Free(id);
    }
    
    public static void Free(GenIdx id, int layer)
    {
        _storage.Free(id, layer);
    }

    public static void EnsureLayerCapacity(int capacity)
    {

    }
}
