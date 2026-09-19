using System.Diagnostics.CodeAnalysis;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel<TNode, TNet, TRule>
{
    public bool TryGetNode(NodeVoxelHandle voxel, [NotNullWhen(true)] out TNode? node)
        => TryGetNode(voxel.Chunk, voxel.Pos, voxel.Layer, out node);
    
    public bool TryGetNode(NodeChunkHandle chunk, Int3 pos, int layer, [NotNullWhen(true)] out TNode? node)
    {
        node = default;
        var genId = GetId(chunk, pos);
        if (!genId.IsValid)
            return false;
        
        node = NodeStorage.Get(genId, layer);
        return true;
    }
    
    public bool TryGetRelative(NodeVoxelHandle node, Int3 offset, int layer, out NodeVoxelHandle relative)
    {
        var chunk = _chunkMap[node.Chunk];
        var targetPos = node.Pos + offset;
        if (targetPos.IsWithin(default, chunk.Dimensions))
        {
            // Same chunk
            relative = new NodeVoxelHandle(node.Chunk, targetPos, layer);
            return true;
        }

        // TODO inter-chunk interactions
        relative = default;
        return false;
    }
    
    public bool TryGetRelative(NodeVoxelHandle node, Int3 offset, out NodeVoxelHandle relative)
    {
        var chunk = _chunkMap[node.Chunk];
        var targetPos = node.Pos + offset;
        if (targetPos.IsWithin(default, chunk.Dimensions))
        {
            // Same chunk
            relative = node with { Pos = targetPos };
            return true;
        }

        // TODO inter-chunk interactions
        relative = default;
        return false;
    }
}
