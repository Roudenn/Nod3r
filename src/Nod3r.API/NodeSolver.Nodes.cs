using JetBrains.Annotations;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.API;

public sealed partial class NodeSolver
{
    [PublicAPI]
    public bool RemoveNode(NodeVoxel voxel)
    {
        return GetKernel(voxel).RemoveNode(voxel.Handle);
    }
    
    [PublicAPI]
    public bool HasNode(NodeVoxel voxel)
    {
        return GetKernel(voxel).HasNode(voxel.Handle);
    }
    
    /// <inheritdoc cref="INodeSolver.TryGetRelative(NodeVoxel, Int3, int, out NodeVoxel)" />
    public bool TryGetRelative(NodeVoxel voxel, Int3 offset, int layer, out NodeVoxel relative)
    {
        var success = GetKernel(voxel).TryGetRelative(voxel.Handle, offset, layer, out var relativeHandle);
        relative = voxel with { Handle = relativeHandle };
        return success;
    }

    /// <inheritdoc cref="INodeSolver.TryGetRelative(NodeVoxel, Int3, out NodeVoxel)" />
    public bool TryGetRelative(NodeVoxel voxel, Int3 offset, out NodeVoxel relative)
    {
        var success = GetKernel(voxel).TryGetRelative(voxel.Handle, offset, out var relativeHandle);
        relative = voxel with { Handle = relativeHandle };
        return success;
    }

    public bool DirtyVoxel(NodeVoxel voxel)
    {
        if (!HasNode(voxel))
            return false;

        // TODO
        return true;
    }
}
