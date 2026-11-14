using JetBrains.Annotations;
using Numos.Maths;

namespace Nod3r.API.Simulations;

public sealed partial class NodeSimulation
{
    /// <summary>
    /// Sets an ID node to be placed at a specific voxel in the simulation.
    /// </summary>
    /// <param name="id">ID of the node.</param>
    /// <param name="voxel">Voxel reference.</param>
    [PublicAPI]
    public void SetNode(int id, VoxelRef voxel)
    {
        GetIDKernel(id).SetNode(voxel.Handle);
    }
    
    /// <summary>
    /// Removes an ID node from a specific voxel in the simulation.
    /// </summary>
    /// <param name="id">ID of the node.</param>
    /// <param name="voxel">Voxel reference.</param>
    /// <returns>
    /// True if the node existed and was removed successfully;
    /// otherwise false.
    /// </returns>
    [PublicAPI]
    public bool RemoveNode(int id, VoxelRef voxel)
    {
        return GetIDKernel(id).RemoveNode(voxel.Handle);
    }
    
    /// <summary>
    /// Checks whether an ID node exists at a certain position in the simulation.
    /// </summary>
    /// <param name="id">ID of the node.</param>
    /// <param name="voxel">Voxel reference.</param>
    /// <returns>True if the node exists; otherwise false.</returns>
    [PublicAPI]
    public bool HasNode(int id, VoxelRef voxel)
    {
        return GetIDKernel(id).HasNode(voxel.Handle);
    }

    /// <summary>
    /// Gets a node voxel of a certain type that is relative
    /// to another voxel on the specified layer, and ensures that it exists.
    /// </summary>
    /// <param name="id">ID of the node type.</param>
    /// <param name="voxel">The origin node.</param>
    /// <param name="offset">Offset relative to the origin voxel to check for the target.</param>
    /// <param name="relative">The node that was found at that position.</param>
    /// <returns>True if the node was found on a relative position.</returns>
    [PublicAPI]
    public bool TryGetRelative(int id, VoxelRef voxel, Int3 offset, out VoxelRef relative)
    {
        var success = GetIDKernel(id).TryGetRelative(voxel.Handle, offset, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }
}
