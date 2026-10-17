using Nod3r.Collections;
using Numos.Maths;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel<TNode, TNet>
{
    public void AddNode(TNode node, Int3 chunk, Int3 pos)
    {
        ushort flatIndex = (ushort) FlatArrayHelpers.GetIndex(Dimensions, pos);
        byte layer = NodeStorage.GetFreeLayer(GetId(chunk, flatIndex));
        SetNode(node, chunk, flatIndex, layer);
    }

    public void SetNode(TNode node, VoxelHandle handle)
    {
        SetNode(node, handle.ChunkPos, handle.VoxelIndex, handle.Layer);
    }

    public void SetNode(TNode node, Int3 chunkPos, ushort voxelIndex, byte layer)
    {
        var chunk = GetChunk(chunkPos);
        var oldGenId = chunk.Handles[voxelIndex];
        ColumnHandle handle;
        if (oldGenId.IsValid)
        {
            // Overwrite the existing layer if it is specified
            NodeStorage.Set(NodeStorage.GetLayerId(oldGenId, layer), node);
            handle = oldGenId;
        }
        else
        {
            NodeStorage.Add(node, layer, out var newId);
            handle = newId.ColumnHandle;
        }
        
        chunk.Handles[chunkPos] = handle;
    }

    public bool RemoveNode(VoxelHandle handle)
    {
        if (!TryGetId(handle, out var id))
            return false;
        
        if (!TryGetNode(handle, out var node))
            return false;
        
        NodeStorage.Free(id, handle.Layer);
        GetChunk(handle.ChunkPos).Handles[handle.VoxelIndex] = ColumnHandle.Invalid;
        return true;
    }

    public bool HasNode(VoxelHandle handle)
    {
        return GetChunk(handle.ChunkPos).Handles[handle.VoxelIndex].IsValid;
    }
}
