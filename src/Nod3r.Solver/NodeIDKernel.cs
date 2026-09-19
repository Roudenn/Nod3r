using System.Collections.Concurrent;
using Nod3r.Collections;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Kernel implementation for node IDs.
/// </summary>
internal sealed partial class NodeIDKernel<TNet, TRule>
    (INodeSolver solver) : INodeIDKernel
    where TNet : INodeNet, INodeNetCreator<TNet>
    where TRule : INodeRule, INodeRuleCreator<TRule>
{
    /// <summary>
    /// Owning solver of this kernel instance.
    /// Required for node rules, since they want access to the whole
    /// node map and not just the current kernel.
    /// </summary>
    private readonly INodeSolver _solver = solver;
    
    private readonly ConcurrentDictionary<NodeChunkHandle, NodeIDChunk> _chunkMap = new();
    
    public List<GenId> Nets { get; set; } = new();
    
    public NodeNetStorage<TNet> NodeNetStorage { get; set; } = new();
    
    public void SetNode(NodeVoxelHandle voxel)
    {
        _chunkMap[voxel.Chunk].Bytes[voxel.Pos] = true;
    }
    
    public bool HasNode(NodeVoxelHandle voxel)
    {
        return _chunkMap[voxel.Chunk].Bytes[voxel.Pos];
    }
    
    public bool RemoveNode(NodeVoxelHandle voxel)
    {
        var hadNode = HasNode(voxel);
        _chunkMap[voxel.Chunk].Bytes[voxel.Pos] = false;
        return hadNode;
    }
    
    // TODO this is copy-pasted code from NodeKernel
    #region copy pasteee

    public void CreateChunk(Int3 position, int width, int height, int depth)
    {
        var chunks = new NodeIDChunk(width, height, depth);
        _chunkMap.TryAdd(new NodeChunkHandle(position), chunks);
    }
    
    public bool HasChunk(Int3 position)
    {
        return _chunkMap.ContainsKey(new NodeChunkHandle(position));
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
    
    #endregion
}
