using JetBrains.Annotations;
using Nod3r.API.Snapshots;
using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.API;

/// <summary>
/// A solver instance that allows manipulating a chunk map of nodes
/// and rebuild node networks inside of it.
/// </summary>
/// <para>
/// A solver is a continuous grid where grids can interact with each other directly.
/// Multiple simulation instances can be connected together in context of a <see cref="NodeWorld"/>
/// with explicit links.
/// </para>
/// <para>
/// Each solver wraps a node kernel instance for every registered node type <see cref="NodeIdx"/>.
/// Such division right at the top makes the internals much nicer and faster.
/// </para>
public sealed partial class NodeSimulation : INodeSimulation, INodeRegistration
{
    private INodeKernel[] _kernels = [];

    private readonly SortedSet<NodeChunkHandle> _registeredChunks = new();

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
    
    [PublicAPI]
    public NodeWorld World { get; }
    
    [PublicAPI]
    public SimulationId Id { get; }
    
    [PublicAPI]
    public Int3 Dimensions { get; }
    
    public NodeSimulation(
        NodeWorld world,
        int chunkDepth = NodeChunkConstants.DefaultDepth,
        int chunkHeight = NodeChunkConstants.DefaultHeight,
        int chunkWidth = NodeChunkConstants.DefaultWidth)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkHeight);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkDepth);
        
        World = world;
        Dimensions = new Int3(chunkWidth, chunkHeight, chunkDepth);
        Id = world.RegisterSimulation(this);
        
        world.Config.RegistrationDelegate.Invoke(this);
    }

    private INodeKernel GetKernel(VoxelRef voxel) => _kernels[_idxes[voxel.Type.Value]];
    
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
    
    [PublicAPI]
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
    [PublicAPI]
    public NodeChunkSnapshot<T>[] GetChunksSnapshot<T>() where T : INode
    {
        // TODO Parallelize
        var kernel = GetKernel<T>();
        var handles = GetChunkHandles();
        var array = new NodeChunkSnapshot<T>[kernel.ChunkCount];
        
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
    [PublicAPI]
    public NodeChunkSnapshot<T> GetChunkSnapshot<T>(NodeChunkHandle handle) where T : INode
    {
        var kernel = GetKernel<T>();
        if (!kernel.HasChunk(handle))
            throw new ArgumentException($"No chunk found at position {handle.Position}!");

        var set = new List<(T Data, Int3 Pos)>();
        kernel.GetChunkData(handle, set, out var dimensions);
        
        return new NodeChunkSnapshot<T>(set, dimensions, handle);
    }

    /// <summary>
    /// Ensures that a chunk exists at a specific position in the local chunk map of a specific kernel.
    /// </summary>
    /// <param name="position">Local chunk map position.</param>
    /// <typeparam name="T">Type of the node of a chunk to ensure.</typeparam>
    /// <returns>A handle to an already existing or newly created chunk.</returns>
    [PublicAPI]
    public NodeChunkHandle EnsureChunk<T>(Int3 position) where T : INode
    {
        var kernel = GetKernel<T>();
        if (!kernel.HasChunk(new NodeChunkHandle(position)))
            kernel.CreateChunk(position);
        
        return new NodeChunkHandle(position);
    }
    
    public NodeChunkHandle EnsureChunk(Int3 position)
    {
        foreach (var kernel in _kernels)
        {
            if (!kernel.HasChunk(position))
                kernel.CreateChunk(position);
        }
        
        var handle = new NodeChunkHandle(position);
        _registeredChunks.Add(handle);
        return handle;
    }
    
    /// <summary>
    /// Creates a <see cref="NodeChunkSnapshot{T}"/> for all chunks
    /// of a certain type in a solver.
    /// </summary>
    /// <param name="id">Node ID to create a snapshot of.</param>
    /// <returns>An array of snapshots of every chunk.</returns>
    [PublicAPI]
    public NodeChunkSnapshot[] GetChunksSnapshot(int id)
    {
        // TODO Parallelize
        var kernel = GetIDKernel(id);
        var handles = GetChunkHandles();
        var array = new NodeChunkSnapshot[kernel.ChunkCount];
        
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
    [PublicAPI]
    public NodeChunkSnapshot GetChunkSnapshot(int id, NodeChunkHandle handle)
    {
        var kernel = GetIDKernel(id);
        if (!kernel.HasChunk(handle))
            throw new ArgumentException($"No chunk found at position {handle.Position}!");

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
    [PublicAPI]
    public NodeChunkHandle EnsureChunk(int id, Int3 position)
    {
        var kernel = GetIDKernel(id);
        var handle = new NodeChunkHandle(position);
        
        if (kernel.HasChunk(handle))
            return handle;
        
        kernel.CreateChunk(position);
        _registeredChunks.Add(handle);
        return handle;
    }

    [PublicAPI]
    public NodeChunkHandle[] GetChunkHandles()
    {
        var arr = new NodeChunkHandle[_registeredChunks.Count];
        _registeredChunks.CopyTo(arr);
        return arr;
    }
}
