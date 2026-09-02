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
    /// General function that updates all node groups in this solver instance.
    /// </summary>
    public void Rebuild()
    {
        _kernel.Rebuild();
    }
    
    public void AddNode<T>(T node, NodeChunkHandle chunk, Int3 pos, bool dirty = true) where T : INode
    {
        var type = _kernel.TypeToIdx<T>();
        var layer = NodeStorage<T>.GetFreeLayer(_kernel.GetId(chunk, pos, type));
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
        SetNode(node, new NodeVoxel(chunk, pos, _kernel.TypeToIdx<T>(), layer), dirty);
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
        return RemoveNode<T>(new NodeVoxel(chunk, pos, _kernel.TypeToIdx<T>(), layer));
    }

    public NodeChunkHandle EnsureChunk(Int3 position)
    {
        if (!_kernel.HasChunk(position))
            _kernel.CreateChunk(position, _chunkWidth, _chunkHeight, _chunkDepth);
        
        return new NodeChunkHandle(position);
    }
}
