using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Nod3r.Solver;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.API;

/// <summary>
/// A solver instance that allows manipulating a chunk map of nodes
/// and rebuild node networks inside of it.
/// <para>
/// Each solver wraps a node kernel instance for every registered node type <see cref="NodeIdx"/>.
/// Such division right at the top makes the internals much nicer and faster.
/// </para>
/// </summary>
public sealed partial class NodeSolver : INodeSolver, INodeRegistration
{
    private INodeKernel[] _kernels = [];
    
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
        
        _chunkDepth = chunkDepth;
        _chunkHeight = chunkHeight;
        _chunkWidth = chunkWidth;
        
        config.RegistrationDelegate.Invoke(this);
        
        Initialized = true;
    }
    
    /// <summary>
    /// Specifies whenever the initialization of the node solver was completed.
    /// This is marked as true after the end of the object constructor.
    /// </summary>
    public bool Initialized { get; private set; } = false;

    /// <summary>
    /// Registered amount of node types in this solver instance.
    /// </summary>
    public int RegistrationCount { get; private set; } = 0;
    
    /// <summary>
    /// All node type indexes that were registered in this solver.
    /// </summary>
    private readonly List<NodeIdx> _registeredIdxs = new();
    
    /// <summary>
    /// General function that updates all node networks in this solver instance.
    /// </summary>
    [PublicAPI]
    public void Rebuild()
    {
        // TODO
    }
    
    [PublicAPI]
    public void AddNode<T>(T node, NodeChunkHandle chunk, Int3 pos) where T : INode
    {
        GetKernel<T>().AddNode(node, chunk, pos);
    }
    
    [PublicAPI]
    public void SetNode<T>(T node, NodeVoxelHandle voxel) where T : INode
    {
        GetKernel<T>().SetNode(node, voxel);
    }
    
    [PublicAPI]
    public void SetNode<T>(T node, NodeChunkHandle chunk, Int3 pos, int layer) where T : INode
    {
        SetNode(node, new NodeVoxelHandle(chunk, pos, layer));
    }

    [PublicAPI]
    public bool RemoveNode<T>(NodeVoxelHandle voxel) where T : INode
    {
        return GetKernel<T>().RemoveNode(voxel);
    }
    
    [PublicAPI]
    public bool RemoveNode<T>(NodeChunkHandle chunk, Int3 pos, int layer) where T : INode
    {
        return RemoveNode<T>(new NodeVoxelHandle(chunk, pos, layer));
    }
    
    [PublicAPI]
    public bool RemoveNode(NodeVoxel voxel)
    {
        return _kernels[voxel.TypeId.Value].RemoveNode(voxel.Handle);
    }
    
    [PublicAPI]
    public bool HasNode(NodeVoxel voxel)
    {
        return _kernels[voxel.TypeId.Value].HasNode(voxel.Handle);
    }
    
    [PublicAPI]
    public bool HasNode<T>(NodeVoxelHandle voxel) where T : INode
    {
        return GetKernel<T>().HasNode(voxel);
    }

    /// <inheritdoc cref="INodeSolver.TryGetNode{T}(NodeVoxelHandle, out T?)" />
    public bool TryGetNode<T>(NodeVoxelHandle voxel, [NotNullWhen(true)] out T? node) where T : INode
    {
        return TryGetNode(voxel.Chunk, voxel.Pos, voxel.Layer, out node);
    }

    public bool TryGetRelative<T>(NodeVoxelHandle voxel, Int3 offset, int layer, out NodeVoxel relative) where T : INode
    {
        var success = GetKernel<T>().TryGetRelative(voxel, offset, layer, out var relativeHandle);
        relative = new NodeVoxel(relativeHandle, NodeIdxStorage.Get<T>());
        return success;
    }

    public bool TryGetRelative<T>(NodeVoxelHandle voxel, Int3 offset, out NodeVoxel relative) where T : INode
    {
        var success = GetKernel<T>().TryGetRelative(voxel, offset, out var relativeHandle);
        relative = new NodeVoxel(relativeHandle, NodeIdxStorage.Get<T>());
        return success;
    }

    /// <inheritdoc cref="INodeSolver.TryGetNode{T}(NodeChunkHandle, Int3, int, out T?)" />
    public bool TryGetNode<T>(NodeChunkHandle chunk, Int3 pos, int layer, [NotNullWhen(true)] out T? node) where T : INode
    {
        return GetKernel<T>().TryGetNode(chunk, pos, layer, out node);
    }

    /// <inheritdoc cref="INodeSolver.TryGetRelative(NodeVoxel, Int3, NodeIdx, int, out NodeVoxel)" />
    public bool TryGetRelative(NodeVoxel voxel, Int3 offset, NodeIdx type, int layer, out NodeVoxel relative)
    {
        var success = GetKernel(voxel).TryGetRelative(voxel.Handle, offset, layer, out var relativeHandle);
        relative = new NodeVoxel(relativeHandle, type);
        return success;
    }

    /// <inheritdoc cref="INodeSolver.TryGetRelative(NodeVoxel, Int3, NodeIdx, out NodeVoxel)" />
    public bool TryGetRelative(NodeVoxel voxel, Int3 offset, NodeIdx type, out NodeVoxel relative)
    {
        var success = GetKernel(voxel).TryGetRelative(voxel.Handle, offset, out var relativeHandle);
        relative = new NodeVoxel(relativeHandle, type);
        return success;
    }

    public bool DirtyVoxel(NodeVoxel voxel)
    {
        if (!HasNode(voxel))
            return false;

        // TODO
        return true;
    }

    public bool DirtyVoxel<T>(NodeVoxelHandle voxel) where T : INode
    {
        if (!HasNode<T>(voxel))
            return false;
        
        // TODO
        return true;
    }

    private INodeKernel GetKernel(NodeVoxel voxel) => _kernels[voxel.TypeId.Value];
    
    private INodeKernel<T> GetKernel<T>() where T : INode => (INodeKernel<T>) _kernels[NodeIdxStorage.Get<T>().Value];
    
    private INodeKernel<TNode, TNet> GetKernelNet<TNode, TNet>()
        where TNode : INode
        where TNet : INodeNet
        => (INodeKernel<TNode, TNet>) _kernels[NodeIdxStorage.Get<TNode>().Value];

    // TODO this should resolve the template automatically instead of forcing to specify the node type explicitly
    /// <summary>
    /// Gets the snapshot of all node networks currently active in this solver instance.
    /// </summary>
    /// <typeparam name="T">Type of the node network to get.</typeparam>
    /// <typeparam name="TNode">Type of the node this network controls.</typeparam>
    /// <returns>A collection of <see cref="NodeNetSnapshot{T}"/>s for every active network.</returns>
    public NodeNetSnapshot<T>[] GetAllNetworks<T, TNode>() where T : INodeNet<TNode> where TNode : INode
    {
        var kernel = GetKernelNet<TNode, T>();
        var handles = kernel.Nets;
        var array = new NodeNetSnapshot<T>[handles.Count];
        for (var i = 0; i < handles.Count; i++)
        {
            var handle = handles[i];
            var net = kernel.NodeNetStorage.Get(handle);
            array[i] = new NodeNetSnapshot<T>(net, handle, net.Nodes);
        }
        
        return array;
    }

    public NodeChunkHandle EnsureChunk<T>(Int3 position) where T : INode
    {
        var kernel = GetKernel<T>();
        if (!kernel.HasChunk(position))
            kernel.CreateChunk(position, _chunkWidth, _chunkHeight, _chunkDepth);
        
        return new NodeChunkHandle(position);
    }
}
