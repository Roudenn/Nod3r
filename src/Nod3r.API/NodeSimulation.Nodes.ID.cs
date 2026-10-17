using JetBrains.Annotations;
using Numos.Maths;

namespace Nod3r.API;

public sealed partial class NodeSimulation
{
    [PublicAPI]
    public void SetNode(int id, VoxelRef voxel)
    {
        GetIDKernel(id).SetNode(voxel.Handle);
    }
    
    [PublicAPI]
    public bool RemoveNode(int id, VoxelRef voxel)
    {
        return GetIDKernel(id).RemoveNode(voxel.Handle);
    }
    
    [PublicAPI]
    public bool HasNode(int id, VoxelRef voxel)
    {
        return GetIDKernel(id).HasNode(voxel.Handle);
    }

    [PublicAPI]
    public bool TryGetRelative(int id, VoxelRef voxel, Int3 offset, out VoxelRef relative)
    {
        var success = GetIDKernel(id).TryGetRelative(voxel.Handle, offset, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }
}
