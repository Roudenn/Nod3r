using JetBrains.Annotations;
using Numos.Maths;

namespace Nod3r.API;

public sealed partial class NodeSimulation
{
    [PublicAPI]
    public bool RemoveNode(VoxelRef voxel)
    {
        return GetKernel(voxel).RemoveNode(voxel.Handle);
    }
    
    [PublicAPI]
    public bool HasNode(VoxelRef voxel)
    {
        return GetKernel(voxel).HasNode(voxel.Handle);
    }
    
    /// <inheritdoc cref="INodeSimulation.TryGetRelative(NodeVoxel, Int3, int, out NodeVoxel)" />
    public bool TryGetRelative(VoxelRef voxel, Int3 offset, byte layer, out VoxelRef relative)
    {
        var success = GetKernel(voxel).TryGetRelative(voxel.Handle, offset, layer, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }

    /// <inheritdoc cref="INodeSimulation.TryGetRelative(NodeVoxel, Int3, out NodeVoxel)" />
    public bool TryGetRelative(VoxelRef voxel, Int3 offset, out VoxelRef relative)
    {
        var success = GetKernel(voxel).TryGetRelative(voxel.Handle, offset, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }

    public bool DirtyVoxel(VoxelRef voxel)
    {
        if (!HasNode(voxel))
            return false;

        // TODO
        return true;
    }
}
