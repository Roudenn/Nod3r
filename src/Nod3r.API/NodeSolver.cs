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

    /// <summary>
    /// Registered amount of node types in this solver instance.
    /// </summary>
    public int RegistrationCount { get; private set; }
    
    /// <summary>
    /// An array that converts <see cref="NodeIdx"/> value into a local index
    /// that can be used to get a proper <see cref="INodeKernel"/>.
    /// </summary>
    private int[] _idxes = [];
    
    /// <summary>
    /// All node type indexes that were registered in this solver.
    /// </summary>
    private readonly List<NodeIdx> _registeredIdxs = new();
    
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
    }

    private INodeKernel GetKernel(NodeVoxel voxel) => _kernels[_idxes[voxel.TypeId.Value]];
    
    private INodeKernel<T> GetKernel<T>() where T : INode => (INodeKernel<T>) _kernels[_idxes[NodeIdxStorage.Get<T>().Value]];

    private INodeIDKernel GetIDKernel(int id) => (INodeIDKernel) _kernels[_idxes[NodeIdxStorage.Get(id).Value]];
    
    private INodeKernel<TNode, TNet> GetKernelNet<TNode, TNet>()
        where TNode : INode
        where TNet : INodeNet
        => (INodeKernel<TNode, TNet>) _kernels[_idxes[NodeIdxStorage.Get<TNode>().Value]];

    // TODO this should resolve the template automatically instead of forcing to specify the node type explicitly
    /// <summary>
    /// Gets the snapshot of all node networks currently active in this solver instance.
    /// </summary>
    /// <typeparam name="T">Type of the node network to get.</typeparam>
    /// <typeparam name="TNode">Type of the node this network controls.</typeparam>
    /// <returns>A collection of <see cref="NodeNetSnapshot{T}"/>s for every active network.</returns>
    public NodeNetSnapshot<T>[] GetNetworksSnapshot<T, TNode>() where T : INodeNet<TNode, T> where TNode : INode
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

    public NodeChunkSnapshot<T>[] GetChunksSnapshot<T>() where T : INode
    {
        // TODO Parallelize
        var kernel = GetKernel<T>();
        var handles = new NodeChunkHandle[kernel.ChunkCount];
        var array = new NodeChunkSnapshot<T>[kernel.ChunkCount];
        kernel.GetChunkHandles(handles);
        
        for (var i = 0; i < handles.Length; i++)
        {
            var handle = handles[i];
            var data = new List<(T Data, Int3 Pos)>();
            kernel.GetChunkData(handle, data, out var dimensions);
            array[i] = new NodeChunkSnapshot<T>(data, dimensions, handle.Pos);
        }

        return array;
    }
    
    public NodeChunkSnapshot<T> GetChunkSnapshot<T>(NodeChunkHandle handle) where T : INode
    {
        var kernel = GetKernel<T>();
        if (!kernel.HasChunk(handle))
            throw new ArgumentException($"No chunk found at position {handle.Pos}!");

        var set = new List<(T Data, Int3 Pos)>();
        kernel.GetChunkData(handle, set, out var dimensions);
        
        return new NodeChunkSnapshot<T>(set, dimensions, handle.Pos);
    }

    public NodeChunkHandle EnsureChunk<T>(Int3 position) where T : INode
    {
        var kernel = GetKernel<T>();
        if (!kernel.HasChunk(new NodeChunkHandle(position)))
            kernel.CreateChunk(position, _chunkWidth, _chunkHeight, _chunkDepth);
        
        return new NodeChunkHandle(position);
    }
    
    public NodeChunkSnapshot[] GetChunksSnapshot(int id)
    {
        // TODO Parallelize
        var kernel = GetIDKernel(id);
        var handles = new NodeChunkHandle[kernel.ChunkCount];
        var array = new NodeChunkSnapshot[kernel.ChunkCount];
        kernel.GetChunkHandles(handles);
        
        for (var i = 0; i < handles.Length; i++)
        {
            var handle = handles[i];
            var data = new HashSet<Int3>();
            kernel.GetChunkData(handle, data, out var dimensions);
            array[i] = new NodeChunkSnapshot(id, data, dimensions, handle.Pos);
        }

        return array;
    }
    
    public NodeChunkSnapshot GetChunkSnapshot(int id, NodeChunkHandle handle)
    {
        var kernel = GetIDKernel(id);
        if (!kernel.HasChunk(handle))
            throw new ArgumentException($"No chunk found at position {handle.Pos}!");

        var set = new HashSet<Int3>();
        kernel.GetChunkData(handle, set, out var dimensions);
        
        return new NodeChunkSnapshot(id, set, dimensions, handle.Pos);
    }

    public NodeChunkHandle EnsureChunk(int id, Int3 position)
    {
        var kernel = GetIDKernel(id);
        if (!kernel.HasChunk(new NodeChunkHandle(position)))
            kernel.CreateChunk(position, _chunkWidth, _chunkHeight, _chunkDepth);
        
        return new NodeChunkHandle(position);
    }
}
