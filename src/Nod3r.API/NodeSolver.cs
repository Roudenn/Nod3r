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
    
    private INodeNetKernel<T> GetIDKernelNet<T>(int id)
        where T : INodeNet
        => (INodeNetKernel<T>) _kernels[_idxes[NodeIdxStorage.Get(id).Value]];
    
    private INodeNetKernel<T> GetKernelNet<T>()
        where T : INodeNet
        => (INodeNetKernel<T>) _kernels[_idxes[NodeIdxStorage.GetNet<T>().Value]];
    
    /// <summary>
    /// Gets the snapshot of all node networks currently active in this solver instance.
    /// </summary>
    /// <remarks>
    /// This works only for type nodes! Use <see cref="GetNetworksSnapshot{T}(int)"/>
    /// overload if you want to get all networks from a specific ID node.
    /// </remarks>
    /// <typeparam name="T">Type of the node network to get.</typeparam>
    /// <returns>A collection of <see cref="NodeNetSnapshot{T}"/>s for every active network.</returns>
    public NodeNetSnapshot<T>[] GetNetworksSnapshot<T>() where T : INodeNet
    {
        var kernel = GetKernelNet<T>();
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
    
    public NodeNetSnapshot<T>[] GetNetworksSnapshot<T>(int id) where T : INodeNet
    {
        var kernel = GetIDKernelNet<T>(id);
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

    /// <summary>
    /// Creates a <see cref="NodeChunkSnapshot{T}"/> for all chunks
    /// of a certain type in a solver.
    /// </summary>
    /// <typeparam name="T">Type of the node to create a snapshot of.</typeparam>
    /// <returns>An array of snapshots of every chunk.</returns>
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
            array[i] = new NodeChunkSnapshot<T>(data, dimensions, handle);
        }

        return array;
    }
    
    /// <summary>
    /// Creates a <see cref="NodeChunkSnapshot{T}"/> for a specific chunk.
    /// </summary>
    /// <param name="handle">The target chunk to create a snapshot.</param>
    /// <typeparam name="T">Type of the node to create a snapshot of.</typeparam>
    /// <returns>A <see cref="NodeChunkSnapshot{T}"/> of the target chunk.</returns>
    /// <exception cref="ArgumentException">The specified <see cref="NodeChunkHandle"/> wasn't initialized.</exception>
    public NodeChunkSnapshot<T> GetChunkSnapshot<T>(NodeChunkHandle handle) where T : INode
    {
        var kernel = GetKernel<T>();
        if (!kernel.HasChunk(handle))
            throw new ArgumentException($"No chunk found at position {handle.Pos}!");

        var set = new List<(T Data, Int3 Pos)>();
        kernel.GetChunkData(handle, set, out var dimensions);
        
        return new NodeChunkSnapshot<T>(set, dimensions, handle);
    }

    /// <summary>
    /// Ensures that a chunk exists at a specific position in the local chunk map.
    /// </summary>
    /// <param name="position">Local chunk map position.</param>
    /// <typeparam name="T">Type of the node of a chunk to ensure.</typeparam>
    /// <returns>A handle to an already existing or newly created chunk.</returns>
    public NodeChunkHandle EnsureChunk<T>(Int3 position) where T : INode
    {
        var kernel = GetKernel<T>();
        if (!kernel.HasChunk(new NodeChunkHandle(position)))
            kernel.CreateChunk(position, _chunkWidth, _chunkHeight, _chunkDepth);
        
        return new NodeChunkHandle(position);
    }
    
    /// <summary>
    /// Creates a <see cref="NodeChunkSnapshot{T}"/> for all chunks
    /// of a certain type in a solver.
    /// </summary>
    /// <param name="id">Node ID to create a snapshot of.</param>
    /// <returns>An array of snapshots of every chunk.</returns>
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
            array[i] = new NodeChunkSnapshot(id, data, dimensions, handle);
        }

        return array;
    }
    
    /// <summary>
    /// Creates a <see cref="NodeChunkSnapshot{T}"/> for a specific chunk.
    /// </summary>
    /// <param name="id">Node ID to create a snapshot of.</param>
    /// <param name="handle">The target chunk to create a snapshot.</param>
    /// <returns>A <see cref="NodeChunkSnapshot{T}"/> of the target chunk.</returns>
    /// <exception cref="ArgumentException">The specified <see cref="NodeChunkHandle"/> wasn't initialized.</exception>
    public NodeChunkSnapshot GetChunkSnapshot(int id, NodeChunkHandle handle)
    {
        var kernel = GetIDKernel(id);
        if (!kernel.HasChunk(handle))
            throw new ArgumentException($"No chunk found at position {handle.Pos}!");

        var set = new HashSet<Int3>();
        kernel.GetChunkData(handle, set, out var dimensions);
        
        return new NodeChunkSnapshot(id, set, dimensions, handle);
    }

    /// <summary>
    /// Ensures that a chunk exists at a specific position in the local chunk map.
    /// </summary>
    /// <param name="position">Local chunk map position.</param>
    /// <param name="id">Node ID to ensure a chunk.</param>
    /// <returns>A handle to an already existing or newly created chunk.</returns>
    public NodeChunkHandle EnsureChunk(int id, Int3 position)
    {
        var kernel = GetIDKernel(id);
        if (!kernel.HasChunk(new NodeChunkHandle(position)))
            kernel.CreateChunk(position, _chunkWidth, _chunkHeight, _chunkDepth);
        
        return new NodeChunkHandle(position);
    }
}
