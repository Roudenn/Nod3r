using System.Diagnostics.CodeAnalysis;
using Numos.Maths;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel<TNode, TNet>
{
    public bool TryGetNode(VoxelHandle handle, [NotNullWhen(true)] out TNode? node)
    {
        node = default;
        var genId = GetId(handle.ChunkPos, handle.VoxelIndex);
        if (!genId.IsValid)
            return false;
        
        node = NodeStorage.Get(genId, handle.Layer);
        return true;
    }
    
    public bool TryGetRelative(VoxelHandle node, Int3 offset, byte layer, out VoxelHandle relative)
    {
        var chunk = _chunkMap[node.ChunkPos];
        var targetPos = chunk.Handles.GetPosition(node.VoxelIndex) + offset;
        if (targetPos.IsWithin(default, chunk.Dimensions))
        {
            // Same chunk
            relative = new VoxelHandle(node.ChunkPos, (ushort) chunk.Handles.GetIndex(targetPos), layer);
            return true;
        }

        // TODO inter-chunk interactions
        relative = default;
        return false;
    }
    
    public bool TryGetRelative(VoxelHandle node, Int3 offset, out VoxelHandle relative)
    {
        var chunk = _chunkMap[node.ChunkPos];
        var targetPos = chunk.Handles.GetPosition(node.VoxelIndex) + offset;
        if (targetPos.IsWithin(default, chunk.Dimensions))
        {
            // Same chunk
            relative = node with { VoxelIndex = (ushort) chunk.Handles.GetIndex(targetPos) };
            return true;
        }

        // TODO inter-chunk interactions
        relative = default;
        return false;
    }
}
