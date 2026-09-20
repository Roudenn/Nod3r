using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

// Contains API methods to interact with chunks.
internal sealed partial class NodeKernel<TNode, TNet, TRule>
{
    public bool HasChunk(NodeChunkHandle handle)
    {
        return _chunkMap.ContainsKey(handle);
    }
    
    public void CreateChunk(Int3 position, int width, int height, int depth)
    {
        var chunks = new NodeChunk(width, height, depth);
        _chunkMap.TryAdd(new NodeChunkHandle(position), chunks);
    }

    public void GetChunkHandles(NodeChunkHandle[] set)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(set.Length, _chunkMap.Count);
        _chunkMap.Keys.CopyTo(set, 0);
    }

    public void GetChunkData(NodeChunkHandle handle, List<(TNode Data, Int3 Pos)> set, out Int3 dimensions)
    {
        var chunk = GetChunk(handle);
        var cache = new List<TNode>(NodeStorage.LayerCapacity);
        dimensions = chunk.Dimensions;
        for (int z = 0; z < chunk.Dimensions.Z; z++)
        {
            for (int y = 0; y < chunk.Dimensions.Y; y++)
            {
                for (int x = 0; x < chunk.Dimensions.X; x++)
                {
                    var pos = new Int3(x, y, z);
                    var column = chunk.Handles[pos];
                    if (!column.IsValid)
                        continue;
                    
                    NodeStorage.GetColumnData(column, cache);
                    foreach (var data in cache)
                    {
                        set.Add((data, pos));
                    }
                }
            }
        }
    }
}
