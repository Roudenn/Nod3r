using System.Collections.Concurrent;
using Nod3r.Collections;
using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Kernel implementation for specific <see cref="INode"/> types.
/// </summary>
internal sealed partial class NodeKernel<TNode, TNet>(Int3 dimensions) : IDisposable, INodeKernel<TNode>, INodeNetKernel<TNet>
    where TNode : INode
    where TNet : INodeNet, INodeNetCreator<TNet>
{
    public NodeStorage<TNode> NodeStorage { get; set; } = new();
    
    public NodeNetStorage<TNet> NodeNetStorage { get; set; } = new();
    
    private readonly ConcurrentDictionary<Int3, NodeChunk> _chunkMap = new();
    
    internal readonly Int3 Dimensions = dimensions;
    
    public List<GenId> Nets { get; set; } = new();
    
    public long CollectionRevision;
    
    public int NextDenseId;
    
    // Reused before minting a new id, so DenseId stays bounded by peak concurrent chunk count rather
    // than growing forever. See AtmosChunk.DenseId's remarks on why recycling ids is safe.
    public readonly Stack<int> FreeDenseIds = new();
    
    /// <summary>
    ///     Gets the number of chunks currently registered with the kernel.
    /// </summary>
    /// <remarks>The count includes both awake and sleeping chunks.</remarks>
    public int ChunkCount => _chunkMap.Count;
    
    /// <summary>
    /// Gets the <see cref="GenId"/> for <see cref="NodeStorage{T}"/>
    /// from chunk coordinates, node position and <see cref="NodeIdx"/> of the node.
    /// </summary>
    /// <param name="voxelIndex">Position inside the chunk.</param>
    /// <param name="chunkPos">Coordinates of the chunk.</param>
    /// <returns><see cref="GenId"/> that can be used in the <see cref="NodeStorage{T}"/> to get the node data.</returns>
    public ColumnHandle GetId(Int3 chunkPos, ushort voxelIndex)
    {
        return _chunkMap[chunkPos].Handles[voxelIndex];
    }
    
    /// <summary>
    /// Gets the <see cref="GenId"/> for <see cref="NodeStorage{T}"/> from a <see cref="NodeVoxel"/>.
    /// </summary>
    /// <param name="voxel">The target node voxel.</param>
    /// <returns><see cref="GenId"/> that can be used in the <see cref="NodeStorage{T}"/> to get the node data.</returns>
    public ColumnHandle GetId(VoxelHandle voxel) => GetId(voxel.ChunkPos, voxel.VoxelIndex);

    public bool TryGetId(Int3 chunk, ushort voxelIndex, out ColumnHandle id)
    {
        id = GetId(chunk, voxelIndex);
        return id != ColumnHandle.Invalid;
    }
    
    public bool TryGetId(VoxelHandle voxel, out ColumnHandle id)
    {
        return TryGetId(voxel.ChunkPos, voxel.VoxelIndex, out id);
    }

    public LayerId GetLayerId(ColumnHandle column, int layer)
    {
        return NodeStorage.GetLayerId(column, layer);
    }
}
