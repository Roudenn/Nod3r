using System.Diagnostics.CodeAnalysis;
using Nod3r.Collections;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel
{
    public bool TryGetNode<T>(NodeVoxel voxel, [NotNullWhen(true)] out T? node) where T : INode
        => TryGetNode(voxel.Chunk, voxel.Pos, voxel.TypeId, voxel.Layer, out node);
    
    public bool TryGetNode<T>(NodeChunkHandle chunk, Int3 pos, NodeIdx type, int layer, [NotNullWhen(true)] out T? node) where T : INode
    {
        node = default;
        var genId = GetId(chunk, pos, type);
        if (!genId.IsValid)
            return false;
        
        node = NodeStorage<T>.Get(genId, layer);
        return true;
    }
    
    public bool TryGetRelative(NodeVoxel node, Int3 offset, NodeIdx type, int layer, out NodeVoxel relative)
    {
        var chunk = _chunkMap[node.Chunk.Pos][type.Value];
        var targetPos = node.Pos + offset;
        if (targetPos.IsWithin(default, chunk.Dimensions))
        {
            // Same chunk
            relative = new NodeVoxel(node.Chunk, targetPos, type, layer);
            return true;
        }

        // TODO inter-chunk interactions
        relative = default;
        return false;
    }
    
    public bool TryGetRelative(NodeVoxel node, Int3 offset, NodeIdx type, out NodeVoxel relative)
    {
        var chunk = _chunkMap[node.Chunk.Pos][type.Value];
        var targetPos = node.Pos + offset;
        if (targetPos.IsWithin(default, chunk.Dimensions))
        {
            // Same chunk
            relative = node with { Pos = targetPos, TypeId = type };
            return true;
        }

        // TODO inter-chunk interactions
        relative = default;
        return false;
    }
}
