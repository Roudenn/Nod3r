using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Nod3r.API.Registration;
using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.API.Simulations;

public sealed partial class NodeSimulation
{
    /// <summary>
    /// Adds a node on the first available layer at a specific chunk and position.
    /// </summary>
    /// <param name="node">Node data to set.</param>
    /// <param name="chunk">Target chunk handle.</param>
    /// <param name="voxelPos">Voxel position inside the chunk.</param>
    /// <typeparam name="T">Type of the node data.</typeparam>
    [PublicAPI]
    public VoxelRef AddNode<T>(T node, NodeChunkHandle chunk, Int3 voxelPos) where T : INode
    {
        var handle = GetKernel<T>().AddNode(node, chunk, voxelPos);
        return new VoxelRef(handle, Id, NodeIdxStorage.Get<T>());
    }
    
    /// <summary>
    /// Adds a node on the first available layer at a specific chunk and position.
    /// </summary>
    /// <param name="node">Node data to set.</param>
    /// <param name="voxel">Voxel reference to set the node to.</param>
    /// <typeparam name="T">Type of the node data.</typeparam>
    [PublicAPI]
    public void SetNode<T>(T node, VoxelRef voxel) where T : INode
    {
        GetKernel<T>().SetNode(node, voxel.Handle);
    }
    
    /// <summary>
    /// Sets a node at a specific chunk and position.
    /// </summary>
    /// <param name="node">Node data to set.</param>
    /// <param name="chunk">Target chunk handle.</param>
    /// <param name="voxelPos">Voxel position inside the chunk.</param>
    /// <param name="layer"></param>
    /// <typeparam name="T">Type of the node data.</typeparam>
    /// <returns>A voxel reference to the target node.</returns>
    [PublicAPI]
    public VoxelRef SetNode<T>(T node, NodeChunkHandle chunk, Int3 voxelPos, byte layer = 0) where T : INode
    {
        var flatIndex = (ushort) FlatArrayHelpers.GetIndex(Dimensions, voxelPos);
        var handle = new VoxelHandle(chunk, flatIndex, layer);
        GetKernel<T>().SetNode(node, handle);
        return new VoxelRef(handle, Id, NodeIdxStorage.Get<T>());
    }

    /// <summary>
    /// Removes a node at a specific voxel reference.
    /// </summary>
    /// <param name="voxel">The voxel reference of the node.</param>
    /// <typeparam name="T">Type of the node data.</typeparam>
    /// <returns>True if the node existed at the specified position; otherwise false.</returns>
    [PublicAPI]
    public bool RemoveNode<T>(VoxelRef voxel) where T : INode
    {
        return GetKernel<T>().RemoveNode(voxel.Handle);
    }
    
    /// <summary>
    /// Checks whether a node exists at a voxel reference.
    /// </summary>
    /// <param name="voxel">The voxel reference of the node.</param>
    /// <typeparam name="T">Type of the node data.</typeparam>
    /// <returns>True if the node exists at the specified position; otherwise false.</returns>
    [PublicAPI]
    public bool HasNode<T>(VoxelRef voxel) where T : INode
    {
        return GetKernel<T>().HasNode(voxel.Handle);
    }
    
    internal bool HasNode<T>(VoxelHandle handle) where T : INode
    {
        return GetKernel<T>().HasNode(handle);
    }
    
    /// <summary>
    /// Checks whether a node exists at a chunk, voxel position and layer.
    /// </summary>
    /// <param name="chunk">Chunk handle of the voxel.</param>
    /// <param name="voxelPos">Voxel position inside the chunk.</param>
    /// <param name="layer">Layer of the voxel.</param>
    /// <typeparam name="T">Type of the node data.</typeparam>
    /// <returns>True if the node exists at the specified position; otherwise false.</returns>
    [PublicAPI]
    public bool HasNode<T>(NodeChunkHandle chunk, Int3 voxelPos, byte layer) where T : INode
    {
        var flatIndex = (ushort) FlatArrayHelpers.GetIndex(Dimensions, voxelPos);
        return GetKernel<T>().HasNode(new VoxelHandle(chunk.Position, flatIndex, layer));
    }

    /// <summary>
    /// Attempts to get a node at a specified voxel.
    /// </summary>
    /// <param name="voxel"></param>
    /// <param name="node">The found node.</param>
    /// <typeparam name="T">Type of the node.</typeparam>
    /// <returns>True if the node was found.</returns>
    [PublicAPI]
    public bool TryGetNode<T>(VoxelRef voxel, [NotNullWhen(true)] out T? node) where T : INode
    {
        return GetKernel<T>().TryGetNode(voxel.Handle, out node);
    }

    /// <summary>
    /// Gets a node voxel of a certain type that is relative
    /// to another voxel on the specified layer, and ensures that it exists.
    /// </summary>
    /// <param name="voxel">The origin node.</param>
    /// <param name="offset">Offset relative to the origin voxel to check for the target.</param>
    /// <param name="layer">Target layer to search the node in.</param>
    /// <param name="relative">The node that was found at that position.</param>
    /// <returns>True if the node was found on a relative position.</returns>
    [PublicAPI]
    public bool TryGetRelative<T>(VoxelRef voxel, Int3 offset, byte layer, out VoxelRef relative) where T : INode
    {
        var success = GetKernel<T>().TryGetRelative(voxel.Handle, offset, layer, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }

    /// <summary>
    /// Gets a node voxel of a certain type that is relative
    /// to another voxel on the same layer as the origin, and ensures that it exists.
    /// </summary>
    /// <param name="voxel">The origin node.</param>
    /// <param name="offset">Offset relative to the origin voxel to check for the target.</param>
    /// <param name="relative">The node that was found at that position.</param>
    /// <returns>True if the node was found on a relative position.</returns>
    [PublicAPI]
    public bool TryGetRelative<T>(VoxelRef voxel, Int3 offset, out VoxelRef relative) where T : INode
    {
        var success = GetKernel<T>().TryGetRelative(voxel.Handle, offset, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }
    
    /// <summary>
    /// Marks the target voxel as dirty, which means it itself or one of its neighbors were changed.
    /// </summary>
    /// <param name="voxel">The target voxel to mark as dirty.</param>
    /// <returns>True if the voxel existed and marked as dirty, false if the voxel doesn't exist.</returns>
    [PublicAPI]
    public bool DirtyVoxel<T>(VoxelRef voxel) where T : INode
    {
        return DirtyVoxel<T>(voxel.Handle);
    }
    
    internal bool DirtyVoxel<T>(VoxelHandle handle) where T : INode
    {
        if (!HasNode<T>(handle))
            return false;
        
        // TODO
        return true;
    }
}
