using JetBrains.Annotations;
using Nod3r.API.Snapshots;
using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.API.Simulations;

public sealed partial class NodeSimulation
{
    /// <summary>
    /// Gets the snapshot of all typed node networks currently active in this solver instance.
    /// </summary>
    /// <remarks>
    /// This works only for type nodes! Use <see cref="GetNetworksSnapshot{T}(int)"/>
    /// overload if you want to get all networks from a specific ID node.
    /// </remarks>
    /// <typeparam name="T">Type of the node network to get.</typeparam>
    /// <returns>A collection of <see cref="NodeNetSnapshot{T}"/>s for every active network.</returns>
    public NodeNetSnapshot<T>[] GetNetworksSnapshot<T>() where T : INodeNet<T>
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
    
    /// <summary>
    /// Gets the snapshot of all ID node networks currently active in this solver instance.
    /// </summary>
    /// <param name="id">Target node ID.</param>
    /// <typeparam name="T">Type of the network registered.</typeparam>
    /// <returns>A collection of <see cref="NodeNetSnapshot{T}"/>s for every active network.</returns>
    [PublicAPI]
    public NodeNetSnapshot<T>[] GetNetworksSnapshot<T>(int id) where T : INodeNet<T>
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
    /// Creates a <see cref="NodeChunkSnapshot"/> for all chunks
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
}