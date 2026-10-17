using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.API;

public sealed partial class NodeSimulation
{
    [PublicAPI]
    public void AddNode<T>(T node, NodeChunkHandle chunk, Int3 pos) where T : INode
    {
        GetKernel<T>().AddNode(node, chunk, pos);
    }
    
    [PublicAPI]
    public void SetNode<T>(T node, VoxelRef voxel) where T : INode
    {
        GetKernel<T>().SetNode(node, voxel.Handle);
    }
    
    [PublicAPI]
    public void SetNode<T>(T node, NodeChunkHandle chunk, Int3 voxelPos, byte layer = 0) where T : INode
    {
        ushort flatIndex = (ushort) FlatArrayHelpers.GetIndex(Dimensions, voxelPos);
        GetKernel<T>().SetNode(node, new VoxelHandle(chunk, flatIndex, layer));
    }

    [PublicAPI]
    public bool RemoveNode<T>(VoxelRef voxel) where T : INode
    {
        return GetKernel<T>().RemoveNode(voxel.Handle);
    }
    
    [PublicAPI]
    public bool HasNode<T>(VoxelRef voxel) where T : INode
    {
        return GetKernel<T>().HasNode(voxel.Handle);
    }

    /// <inheritdoc cref="INodeSimulation.TryGetNode{T}(VoxelHandle, out T?)" />
    [PublicAPI]
    public bool TryGetNode<T>(VoxelRef voxel, [NotNullWhen(true)] out T? node) where T : INode
    {
        return GetKernel<T>().TryGetNode(voxel.Handle, out node);
    }

    [PublicAPI]
    public bool TryGetRelative<T>(VoxelRef voxel, Int3 offset, byte layer, out VoxelRef relative) where T : INode
    {
        var success = GetKernel<T>().TryGetRelative(voxel.Handle, offset, layer, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }

    [PublicAPI]
    public bool TryGetRelative<T>(VoxelRef voxel, Int3 offset, out VoxelRef relative) where T : INode
    {
        var success = GetKernel<T>().TryGetRelative(voxel.Handle, offset, out var relativeHandle);
        relative = new VoxelRef(relativeHandle, voxel.Simulation, voxel.Type);
        return success;
    }
    
    [PublicAPI]
    public bool DirtyVoxel<T>(VoxelRef voxel) where T : INode
    {
        if (!HasNode<T>(voxel))
            return false;
        
        // TODO
        return true;
    }
}
