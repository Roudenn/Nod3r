using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// Shared storage for all registered <see cref="NodeIdx"/>es.
/// </summary>
public static class NodeIdxStorage
{
    /// <summary>
    /// Amount of nodes registered in the program.
    /// </summary>
    public static int Count { get; private set; }

    /// <summary>
    /// <see cref="NodeIdx"/>es for ID nodes.
    /// </summary>
    private static NodeIdx[] _idNodes = [];
    
    /// <summary>
    /// Registers a node type and its network in the program, or returns an already registered <see cref="NodeIdx"/>
    /// if it was already registered in the program before.
    /// </summary>
    /// <typeparam name="TNode">Type of node.</typeparam>
    /// <typeparam name="TNet">Type of network that controls <see cref="TNode"/>.</typeparam>
    internal static void Register<TNode, TNet>(out NodeIdx typeIdx)
        where TNode : INode
        where TNet : INodeNet<TNode, TNet>
    {
        if (Storage<TNode>.Index.IsValid)
        {
            typeIdx = Storage<TNode>.Index;
            return;
        }
        
        typeIdx = new NodeIdx(Count);
        Storage<TNode>.Index = typeIdx;
        StorageNet<TNet>.Index = typeIdx;
        Count++;
    }
    
    internal static void Register<TNet>(int id, out NodeIdx typeIdx) where TNet : INodeNet
    {
        if (_idNodes.Length > id && _idNodes[id].IsValid)
        {
            typeIdx = _idNodes[id];
            return;
        }
        
        typeIdx = new NodeIdx(Count);
        EnsureIdCapacity(id);
        _idNodes[id] = typeIdx;
        StorageNet<TNet>.Index = typeIdx;
        Count++;
    }

    private static void EnsureIdCapacity(int capacity)
    {
        var oldLength = _idNodes.Length;
        ArrayHelpers.EnsureCapacity(ref _idNodes, capacity);
        for (int i = oldLength; i < capacity; i++)
        {
            _idNodes[i] = NodeIdx.Invalid;
        }
    }

    /// <summary>
    /// Gets the <see cref="NodeIdx"/> of a node type.
    /// </summary>
    /// <typeparam name="T">Type of node.</typeparam>
    /// <returns><see cref="NodeIdx"/> representing this type.</returns>
    /// <exception cref="InvalidOperationException">The specified type wasn't registered in the program.</exception>
    public static NodeIdx Get<T>() where T : INode
    {
        var idx = Storage<T>.Index;
        return idx == NodeIdx.Invalid
            ? throw new InvalidOperationException($"Tried to get a {nameof(NodeIdx)} for a node type that wasn't registered!")
            : idx;
    }

    /// <summary>
    /// Gets the <see cref="NodeIdx"/> of a node ID.
    /// </summary>
    /// <param name="id">The node ID to get the <see cref="NodeIdx"/> from.</param>
    /// <returns><see cref="NodeIdx"/> representing this type.</returns>
    public static NodeIdx Get(int id)
    {
        return _idNodes[id];
    }
    
    /// <summary>
    /// Gets the <see cref="NodeIdx"/> of a node type controlled by the network.
    /// </summary>
    /// <typeparam name="T">Type of node.</typeparam>
    /// <returns><see cref="NodeIdx"/> representing this type.</returns>
    /// <exception cref="InvalidOperationException">The specified type wasn't registered in the program.</exception>
    public static NodeIdx GetNet<T>() where T : INodeNet
    {
        var idx = StorageNet<T>.Index;
        return idx == NodeIdx.Invalid
            ? throw new InvalidOperationException($"Tried to get a {nameof(NodeIdx)} for a node type that wasn't registered!")
            : idx;
    }
    
    /// <summary>
    /// A helper static class that automatically creates a separate static instance for each registered node type.
    /// </summary>
    /// <typeparam name="T">The controlled node type.</typeparam>
    private static class Storage<T> where T : INode
    {
        // Analyzer suppression is intentional, because here we actually want this field
        // to be different for different generated node types.
        
        // ReSharper disable once StaticMemberInGenericType
        public static NodeIdx Index = NodeIdx.Invalid;
    }
    
    /// <summary>
    /// A helper static class that automatically creates a separate static instance for each registered node network type.
    /// </summary>
    /// <typeparam name="T">The controlled node network type.</typeparam>
    private static class StorageNet<T> where T : INodeNet
    {
        // Analyzer suppression is intentional, because here we actually want this field
        // to be different for different generated node types.
        
        // ReSharper disable once StaticMemberInGenericType
        public static NodeIdx Index = NodeIdx.Invalid;
    }
}
