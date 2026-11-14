using JetBrains.Annotations;
using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.API.Simulations;

public sealed partial class NodeSimulation
{
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
        var handle = new NodeChunkHandle(position);
        
        if (kernel.HasChunk(handle))
            return handle;
        
        kernel.CreateChunk(position);
        _registeredChunks.Add(handle);
        return handle;
    }
    
    /// <summary>
    /// Ensures a chunk in all kernels controlled by this simulation.
    /// </summary>
    [PublicAPI]
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

    /// <summary>
    /// Gets all registered chunk handles.
    /// </summary>
    [PublicAPI]
    public NodeChunkHandle[] GetChunkHandles()
    {
        var arr = new NodeChunkHandle[_registeredChunks.Count];
        _registeredChunks.CopyTo(arr);
        return arr;
    } 
}
