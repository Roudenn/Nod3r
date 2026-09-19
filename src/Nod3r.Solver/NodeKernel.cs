using System.Collections.Concurrent;
using Nod3r.Collections;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Kernel implementation for specific <see cref="INode"/> types.
/// </summary>
internal sealed partial class NodeKernel<TNode, TNet, TRule>
    (INodeSolver solver) : INodeKernel<TNode>
    where TNode : INode
    where TNet : INodeNet, INodeNetCreator<TNet>
    where TRule : INodeRule<TNode>, INodeRuleCreator<TRule>
{
    /// <summary>
    /// Owning solver of this kernel instance.
    /// Required for node rules, since they want access to the whole
    /// node map and not just the current kernel.
    /// </summary>
    private readonly INodeSolver _solver = solver;
    
    public NodeStorage<TNode> NodeStorage { get; set; } = new();
    
    public NodeNetStorage<TNet> NodeNetStorage = new();
    
    private readonly ConcurrentDictionary<NodeChunkHandle, NodeChunk> _chunkMap = new();
    
    public List<GenId> Nets { get; set; } = new();
    
    /// <summary>
    /// Gets the <see cref="GenId"/> for <see cref="NodeStorage{T}"/>
    /// from chunk coordinates, node position and <see cref="NodeIdx"/> of the node.
    /// </summary>
    /// <param name="pos">Position inside the chunk.</param>
    /// <param name="chunk">Coordinates of the chunk.</param>
    /// <returns><see cref="GenId"/> that can be used in the <see cref="NodeStorage{T}"/> to get the node data.</returns>
    public ColumnHandle GetId(NodeChunkHandle chunk, Int3 pos) => _chunkMap[chunk].Handles[pos];
    
    /// <summary>
    /// Gets the <see cref="GenId"/> for <see cref="NodeStorage{T}"/> from a <see cref="NodeVoxel"/>.
    /// </summary>
    /// <param name="voxel">The target node voxel.</param>
    /// <returns><see cref="GenId"/> that can be used in the <see cref="NodeStorage{T}"/> to get the node data.</returns>
    public ColumnHandle GetId(NodeVoxelHandle voxel) => GetId(voxel.Chunk, voxel.Pos);

    public bool TryGetId(NodeChunkHandle chunk, Int3 pos, out ColumnHandle id)
    {
        id = GetId(chunk, pos);
        return id != ColumnHandle.Invalid;
    }
    
    public bool TryGetId(NodeVoxelHandle voxel, out ColumnHandle id)
    {
        return TryGetId(voxel.Chunk, voxel.Pos, out id);
    }

    public LayerId GetLayerId(ColumnHandle column, int layer)
    {
        return NodeStorage.GetLayerId(column, layer);
    }
    
    private NodeChunk GetChunk(NodeChunkHandle chunk) => _chunkMap[chunk];
    
    private NodeChunk GetChunk(NodeVoxelHandle voxel) => _chunkMap[voxel.Chunk];
}
