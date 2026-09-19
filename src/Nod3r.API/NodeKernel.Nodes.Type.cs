using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Nod3r.Solver;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.API;

public sealed partial class NodeSolver
{
    [PublicAPI]
    public void AddNode<T>(T node, NodeChunkHandle chunk, Int3 pos) where T : INode
    {
        GetKernel<T>().AddNode(node, chunk, pos);
    }
    
    [PublicAPI]
    public void SetNode<T>(T node, NodeVoxelHandle voxel) where T : INode
    {
        GetKernel<T>().SetNode(node, voxel);
    }
    
    [PublicAPI]
    public void SetNode<T>(T node, NodeChunkHandle chunk, Int3 pos, int layer) where T : INode
    {
        SetNode(node, new NodeVoxelHandle(chunk, pos, layer));
    }

    [PublicAPI]
    public bool RemoveNode<T>(NodeVoxelHandle voxel) where T : INode
    {
        return GetKernel<T>().RemoveNode(voxel);
    }
    
    [PublicAPI]
    public bool RemoveNode<T>(NodeChunkHandle chunk, Int3 pos, int layer) where T : INode
    {
        return RemoveNode<T>(new NodeVoxelHandle(chunk, pos, layer));
    }
    
    [PublicAPI]
    public bool HasNode<T>(NodeVoxelHandle voxel) where T : INode
    {
        return GetKernel<T>().HasNode(voxel);
    }

    /// <inheritdoc cref="INodeSolver.TryGetNode{T}(NodeVoxelHandle, out T?)" />
    [PublicAPI]
    public bool TryGetNode<T>(NodeVoxelHandle voxel, [NotNullWhen(true)] out T? node) where T : INode
    {
        return TryGetNode(voxel.Chunk, voxel.Pos, voxel.Layer, out node);
    }

    [PublicAPI]
    public bool TryGetRelative<T>(NodeVoxelHandle voxel, Int3 offset, int layer, out NodeVoxel relative) where T : INode
    {
        var success = GetKernel<T>().TryGetRelative(voxel, offset, layer, out var relativeHandle);
        relative = new NodeVoxel(relativeHandle, NodeIdxStorage.Get<T>());
        return success;
    }

    [PublicAPI]
    public bool TryGetRelative<T>(NodeVoxelHandle voxel, Int3 offset, out NodeVoxel relative) where T : INode
    {
        var success = GetKernel<T>().TryGetRelative(voxel, offset, out var relativeHandle);
        relative = new NodeVoxel(relativeHandle, NodeIdxStorage.Get<T>());
        return success;
    }

    /// <inheritdoc cref="INodeSolver.TryGetNode{T}(NodeChunkHandle, Int3, int, out T?)" />
    [PublicAPI]
    public bool TryGetNode<T>(NodeChunkHandle chunk, Int3 pos, int layer, [NotNullWhen(true)] out T? node) where T : INode
    {
        return GetKernel<T>().TryGetNode(chunk, pos, layer, out node);
    }
    
    [PublicAPI]
    public bool DirtyVoxel<T>(NodeVoxelHandle voxel) where T : INode
    {
        if (!HasNode<T>(voxel))
            return false;
        
        // TODO
        return true;
    }
}
