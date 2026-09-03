using Numos.Maths;

namespace Nod3r.Solver;

// Contains API methods to interact with chunks.
internal sealed partial class NodeKernel
{
    public bool HasChunk(Int3 position)
    {
        return _chunkMap.ContainsKey(position);
    }
    
    public void CreateChunk(Int3 position, int width, int height, int depth)
    {
        var chunks = new NodeChunk[NodeIdxStorage.Count];
        Array.Fill(chunks, new NodeChunk(width, height, depth));
        _chunkMap.TryAdd(position, chunks);
    }
}
