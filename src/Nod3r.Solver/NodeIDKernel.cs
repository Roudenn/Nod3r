using System.Collections.Concurrent;
using Nod3r.Collections;
using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Kernel implementation for node IDs.
/// </summary>
internal sealed class NodeIDKernel<TNet>
    (int id, Int3 dimensions) : INodeIDKernel, INodeNetKernel<TNet>
    where TNet : INodeNet, INodeNetCreator<TNet>
{
    internal readonly int ID = id;
    
    private readonly Int3 _dimensions = dimensions;

    private readonly ConcurrentDictionary<Int3, NodeIDChunk> _chunkMap = new();
    
    public List<GenId> Nets { get; set; } = new();
    
    public NodeNetStorage<TNet> NodeNetStorage { get; set; } = new();
    
    public void SetNode(VoxelHandle voxel)
    {
        _chunkMap[voxel.ChunkPos].Bytes[voxel.VoxelIndex] = true;
    }
    
    public bool HasNode(VoxelHandle voxel)
    {
        return _chunkMap[voxel.ChunkPos].Bytes[voxel.VoxelIndex];
    }
    
    public bool RemoveNode(VoxelHandle voxel)
    {
        var hadNode = HasNode(voxel);
        _chunkMap[voxel.ChunkPos].Bytes[voxel.VoxelIndex] = false;
        return hadNode;
    }

    public void GetChunkData(Int3 handle, HashSet<Int3> list, out Int3 dimensions)
    {
        var chunk = GetChunk(handle);
        dimensions = chunk.Dimensions;
        for (int z = 0; z < chunk.Dimensions.Z; z++)
        {
            for (int y = 0; y < chunk.Dimensions.Y; y++)
            {
                for (int x = 0; x < chunk.Dimensions.X; x++)
                {
                    var pos = new Int3(x, y, z);
                    var value = chunk.Bytes[pos];
                    if (value)
                        list.Add(pos);
                }
            }
        }
    }
    
    // TODO this is copy-pasted code from NodeKernel
    #region copy pasteee

    public void CreateChunk(Int3 position)
    {
        var chunks = new NodeIDChunk(position, _dimensions);
        _chunkMap.TryAdd(position, chunks);
    }

    public int ChunkCount => _chunkMap.Count;
    
    public bool HasChunk(Int3 handle)
    {
        return _chunkMap.ContainsKey(handle);
    }

    public bool TryGetRelative(VoxelHandle node, Int3 offset, byte layer, out VoxelHandle relative)
    {
        var chunk = _chunkMap[node.ChunkPos];
        var targetPos = chunk.Bytes.GetPosition(node.VoxelIndex) + offset;
        if (targetPos.IsWithin(default, chunk.Dimensions))
        {
            // Same chunk
            relative = new VoxelHandle(node.ChunkPos, (ushort) chunk.Bytes.GetIndex(targetPos), layer);
            return true;
        }

        // TODO inter-chunk interactions
        relative = default;
        return false;
    }
    
    public bool TryGetRelative(VoxelHandle node, Int3 offset, out VoxelHandle relative)
    {
        var chunk = _chunkMap[node.ChunkPos];
        var targetPos = chunk.Bytes.GetPosition(node.VoxelIndex) + offset;
        if (targetPos.IsWithin(default, chunk.Dimensions))
        {
            // Same chunk
            relative = node with { VoxelIndex = (ushort) chunk.Bytes.GetIndex(targetPos) };
            return true;
        }

        // TODO inter-chunk interactions
        relative = default;
        return false;
    }

    #endregion
    
    private NodeIDChunk GetChunk(Int3 chunk) => _chunkMap[chunk];
    
    private NodeIDChunk GetChunk(VoxelHandle voxel) => _chunkMap[voxel.ChunkPos];
}
