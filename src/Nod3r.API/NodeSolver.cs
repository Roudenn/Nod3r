using System.Diagnostics.CodeAnalysis;
using Nod3r.Solver;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.API;

/// <summary>
/// A solver instance that allows manipulating a chunk map of nodes
/// and rebuild node networks inside of it.
/// <para>
/// Wraps the kernel to provide safe methods for external manipulation with the node chunk map
/// </para>
/// </summary>
public sealed class NodeSolver
{
    private readonly NodeKernel _kernel;
    
    private readonly int _chunkDepth;
    private readonly int _chunkHeight;
    private readonly int _chunkWidth;

    public NodeSolver(
        NodeConfig config,
        int chunkDepth = NodeChunkConstants.DefaultDepth,
        int chunkHeight = NodeChunkConstants.DefaultHeight,
        int chunkWidth = NodeChunkConstants.DefaultWidth)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkHeight);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkDepth);
        
        _kernel = new NodeKernel(config);
        _chunkDepth = chunkDepth;
        _chunkHeight = chunkHeight;
        _chunkWidth = chunkWidth;
    }
    
    /// <summary>
    /// General function that updates all node networks in this solver instance.
    /// </summary>
    public void Rebuild()
    {
        _kernel.Rebuild();
    }
    
    public void AddNode<T>(T node, NodeChunkHandle chunk, Int3 pos, bool dirty = true) where T : INode
    {
        var type = _kernel.NodeTypeToIdx<T>();
        int layer = NodeStorage<T>.GetFreeLayer(_kernel.GetId(chunk, pos, type));
        SetNode(node, new NodeVoxel(chunk, pos, type, layer), dirty);
    }
    
    public void SetNode<T>(T node, NodeVoxel voxel, bool dirty = true) where T : INode
    {
        _kernel.SetNode(node, voxel);
        if (dirty)
            _kernel.DirtyNode(voxel);
    }
    
    public void SetNode<T>(T node, NodeChunkHandle chunk, Int3 pos, int layer, bool dirty = true) where T : INode
    {
        SetNode(node, new NodeVoxel(chunk, pos, _kernel.NodeTypeToIdx<T>(), layer), dirty);
    }

    public bool RemoveNode<T>(NodeVoxel voxel) where T : INode
    {
        if (!_kernel.RemoveNode<T>(voxel))
            return false;
        
        _kernel.DirtyNode(voxel);
        return true;
    }
    
    public bool RemoveNode<T>(NodeChunkHandle chunk, Int3 pos, int layer) where T : INode
    {
        return RemoveNode<T>(new NodeVoxel(chunk, pos, _kernel.NodeTypeToIdx<T>(), layer));
    }

    /// <inheritdoc cref="INodeKernel.TryGetNode{T}(NodeVoxel, out T?)" />
    public bool TryGetNode<T>(NodeVoxel voxel, [NotNullWhen(true)] out T? node) where T : INode
    {
        return _kernel.TryGetNode(voxel, out node);
    }

    /// <inheritdoc cref="INodeKernel.TryGetNode{T}(NodeChunkHandle, Int3, NodeIdx, int, out T?)" />
    public bool TryGetNode<T>(NodeChunkHandle chunk, Int3 pos, NodeIdx type, int layer, [NotNullWhen(true)] out T? node) where T : INode
    {
        return _kernel.TryGetNode(chunk, pos, type, layer, out node);
    }

    /// <inheritdoc cref="INodeKernel.TryGetRelative(NodeVoxel, Int3, NodeIdx, int, out NodeVoxel)" />
    public bool TryGetRelative(NodeVoxel voxel, Int3 offset, NodeIdx type, int layer, out NodeVoxel relative)
    {
        return _kernel.TryGetRelative(voxel, offset, type, layer, out relative);
    }

    /// <inheritdoc cref="INodeKernel.TryGetRelative(NodeVoxel, Int3, NodeIdx, out NodeVoxel)" />
    public bool TryGetRelative(NodeVoxel voxel, Int3 offset, NodeIdx type, out NodeVoxel relative)
    {
        return _kernel.TryGetRelative(voxel, offset, type, out relative);
    }

    /// <summary>
    /// Gets the snapshot of all node networks currently active in this solver instance.
    /// </summary>
    /// <typeparam name="T">Type of the node network to get.</typeparam>
    /// <returns>A collection of <see cref="NodeNetSnapshot{T}"/>s for every active network.</returns>
    public NodeNetSnapshot<T>[] GetAllNetworks<T>() where T : INodeNet
    {
        var handles = _kernel.GetNetHandles(_kernel.NetTypeToIdx<T>());
        var array = new NodeNetSnapshot<T>[handles.Count];
        for (var i = 0; i < handles.Count; i++)
        {
            var handle = handles[i];
            array[i] = new NodeNetSnapshot<T>(NodeNetStorage<T>.Get(handle.GenId), handle.GenId, handle.Nodes);
        }
        
        return array;
    }

    public NodeChunkHandle EnsureChunk(Int3 position)
    {
        if (!_kernel.HasChunk(position))
            _kernel.CreateChunk(position, _chunkWidth, _chunkHeight, _chunkDepth);
        
        return new NodeChunkHandle(position);
    }
}
