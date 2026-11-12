using JetBrains.Annotations;
using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.API.Simulations;

public sealed partial class NodeSimulation
{
    /// <summary>
    /// Removes a node from the simulation.
    /// </summary>
    /// <returns>True if the node was removed successfully; otherwise false.</returns>
    [PublicAPI]
    public bool RemoveNode(VoxelRef voxel)
    {
        return GetKernel(voxel).RemoveNode(voxel.Handle);
    }
    
    /// <summary>
    /// Checks whether a node exists in the simulation.
    /// </summary>
    [PublicAPI]
    public bool HasNode(VoxelRef voxel)
    {
        return GetKernel(voxel).HasNode(voxel.Handle);
    }
    
    
    public bool TryGetRelative(VoxelRef voxel, Int3 offset, byte layer, out VoxelRef relative)
    {
        var success = GetKernel(voxel).TryGetRelative(voxel.Handle, offset, layer, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }
    
    
    public bool TryGetRelative(VoxelRef voxel, Int3 offset, out VoxelRef relative)
    {
        var success = GetKernel(voxel).TryGetRelative(voxel.Handle, offset, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }

    /// <summary>
    /// Marks the voxel as dirty, meaning its
    /// <see cref="NodeRule"/> conditions had changed,
    /// and it has to be checked during the next node network rebuild.
    /// </summary>
    /// <returns>
    /// True if the node exists and was successfully dirtied
    /// for the first time; otherwise false.
    /// </returns>
    public bool DirtyVoxel(VoxelRef voxel)
    {
        if (!HasNode(voxel))
            return false;

        // TODO
        return true;
    }
}
