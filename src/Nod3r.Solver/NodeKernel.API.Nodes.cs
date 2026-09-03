using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

// Contains API methods to interact with nodes.
internal sealed partial class NodeKernel
{
    public void SetNode<T>(T node, NodeVoxel voxel) where T : INode
    {
        var chunk = GetChunk(voxel);
        var oldGenId = chunk.Chunk[voxel.Pos];
        LayerId id;
        if (oldGenId.IsValid)
        {
            // Overwrite the existing layer if it is specified
            NodeStorage<T>.Free(oldGenId, voxel.Layer);
            NodeStorage<T>.Add(node, oldGenId, out id);
        }
        else
        {
            NodeStorage<T>.Add(node, voxel.Layer, out id);
        }
        
        chunk.Chunk[voxel.Pos] = id.ColumnHandle;
        _newNodes.Add(voxel);
        _changedChunks.Add(voxel.Chunk);
    }

    /// <summary>
    /// Removes a node voxel from the chunk map.
    /// </summary>
    /// <param name="voxel">Node voxel to remove.</param>
    /// <typeparam name="T">Type of the node to remove.</typeparam>
    public bool RemoveNode<T>(NodeVoxel voxel) where T : INode
    {
        if (!TryGetId(voxel, out var id))
            return false;
        
        NodeStorage<T>.Free(id, voxel.Layer);
        GetChunk(voxel).Chunk[voxel.Pos] = ColumnHandle.Invalid;
        _changedChunks.Add(voxel.Chunk);
        var neighbors = _ruleFactories[voxel.TypeId.Value].Create().Evaluate(this, voxel);
        foreach (var nearVoxel in neighbors)
        {
            _changedNodes.Add(nearVoxel);
        }

        return true;
    }
    
    /// <summary>
    /// Marks a node voxel as changed, which will force the parent network to update.
    /// Call this method when <see cref="INodeRule"/> have potentially changed.
    /// </summary>
    /// <param name="voxel"></param>
    public void DirtyNode(NodeVoxel voxel)
    {
        _changedNodes.Add(voxel);
    }
}