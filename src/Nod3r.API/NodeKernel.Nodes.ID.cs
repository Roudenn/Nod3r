using JetBrains.Annotations;
using Nod3r.Solver;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.API;

public sealed partial class NodeSolver
{
    [PublicAPI]
    public void SetNode(int id, NodeVoxelHandle voxel)
    {
        GetIDKernel(id).SetNode(voxel);
    }
    
    [PublicAPI]
    public bool RemoveNode(int id, NodeVoxelHandle voxel)
    {
        return GetIDKernel(id).RemoveNode(voxel);
    }
    
    [PublicAPI]
    public bool HasNode(int id, NodeVoxelHandle voxel)
    {
        return GetIDKernel(id).HasNode(voxel);
    }
    
    [PublicAPI]
    public bool TryGetRelative(int id, NodeVoxelHandle voxel, Int3 offset, int layer, out NodeVoxel relative)
    {
        var success = GetIDKernel(id).TryGetRelative(voxel, offset, layer, out var relativeHandle);
        relative = new NodeVoxel(relativeHandle, NodeIdxStorage.Get(id));
        return success;
    }

    [PublicAPI]
    public bool TryGetRelative(int id, NodeVoxelHandle voxel, Int3 offset, out NodeVoxel relative)
    {
        var success = GetIDKernel(id).TryGetRelative(voxel, offset, out var relativeHandle);
        relative = new NodeVoxel(relativeHandle, NodeIdxStorage.Get(id));
        return success;
    }
}
