using Nod3r.Collections;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel<TNode, TNet, TRule>
{
    public void AddNode(TNode node, NodeChunkHandle chunk, Int3 pos)
    {
        int layer = NodeStorage.GetFreeLayer(GetId(chunk, pos));
        SetNode(node, new NodeVoxelHandle(chunk, pos, layer));
    }
    
    public void SetNode(TNode node, NodeVoxelHandle voxel)
    {
        var chunk = GetChunk(voxel);
        var oldGenId = chunk.Handles[voxel.Pos];
        LayerId id;
        if (oldGenId.IsValid)
        {
            // Overwrite the existing layer if it is specified
            NodeStorage.Free(oldGenId, voxel.Layer);
            NodeStorage.Add(node, oldGenId, out id);
        }
        else
        {
            NodeStorage.Add(node, voxel.Layer, out id);
        }
        
        chunk.Handles[voxel.Pos] = id.ColumnHandle;
    }
    
    public bool RemoveNode(NodeVoxelHandle voxel)
    {
        if (!TryGetId(voxel, out var id))
            return false;
        
        if (!TryGetNode(voxel, out var node))
            return false;
        
        NodeStorage.Free(id, voxel.Layer);
        GetChunk(voxel).Handles[voxel.Pos] = ColumnHandle.Invalid;
        
        // TODO: consider making all node rules static
        var neighbors = TRule.CreateRule().Evaluate(_solver, voxel, node);
        foreach (var nearVoxel in neighbors)
        {
            _solver.DirtyVoxel(nearVoxel);
        }

        return true;
    }

    public bool HasNode(NodeVoxelHandle voxel)
    {
        return GetChunk(voxel).Handles[voxel.Pos].IsValid;
    }
}
