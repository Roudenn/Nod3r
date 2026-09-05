using System.Collections.Concurrent;
using Nod3r.Collections;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Solver for 
/// </summary>
internal sealed partial class NodeKernel : INodeKernel, INodeRegistration
{
    public NodeKernel(NodeConfig config)
    {
        config.RegistrationDelegate.Invoke(this);
        //Array.Resize(ref _nets, );
    }

    private NodeStorage[] _nodeStorages = [];
    
    private NodeNetStorage[] _nodeNetStorages = [];
    
    /// <summary>
    /// All currently living networks mapped by <see cref="NodeIdx"/>.
    /// </summary>
    private List<NodeNetInternal>[] _nets = [];
    
    /// <summary>
    /// Factories that create <see cref="INodeNet"/>s instances from <see cref="NodeIdx"/>.
    /// </summary>
    private readonly List<NodeNetFactory> _nodeFactories = new();
    
    /// <summary>
    /// Factories that create <see cref="INodeRule"/>s instances from <see cref="NodeIdx"/>.
    /// </summary>
    private readonly List<NodeRuleFactory> _ruleFactories = new();
    
    private readonly ConcurrentDictionary<Int3, NodeChunk[]> _chunkMap = new();
    
    /// <summary>
    /// Nodes added since the last solve.
    /// </summary>
    private readonly ConcurrentBag<NodeVoxel> _newNodes = new();

    /// <summary>
    /// Nodes that changed their connection conditions and have to rebuild their group.
    /// </summary>
    private readonly ConcurrentBag<NodeVoxel> _changedNodes = new();
    
    /// <summary>
    /// Chunks that got modified since the last solve.
    /// </summary>
    private readonly HashSet<NodeChunkHandle> _changedChunks = new();

    internal List<NodeNetInternal> GetNetHandles(NodeIdx typeId)
    {
        return _nets[typeId.Value];
    }

    /// <summary>
    /// Gets the <see cref="GenId"/> for <see cref="NodeStorage{T}"/>
    /// from chunk coordinates, node position and <see cref="NodeIdx"/> of the node.
    /// </summary>
    /// <param name="pos">Position inside the chunk.</param>
    /// <param name="chunk">Coordinates of the chunk.</param>
    /// <param name="typeId">Node type index.</param>
    /// <returns><see cref="GenId"/> that can be used in the <see cref="NodeStorage{T}"/> to get the node data.</returns>
    public ColumnHandle GetId(NodeChunkHandle chunk, Int3 pos, NodeIdx typeId) => _chunkMap[chunk.Pos][typeId.Value].Chunk[pos];
    
    /// <summary>
    /// Gets the <see cref="GenId"/> for <see cref="NodeStorage{T}"/> from a <see cref="NodeVoxel"/>.
    /// </summary>
    /// <param name="voxel">The target node voxel.</param>
    /// <returns><see cref="GenId"/> that can be used in the <see cref="NodeStorage{T}"/> to get the node data.</returns>
    public ColumnHandle GetId(NodeVoxel voxel) => GetId(voxel.Chunk, voxel.Pos, voxel.TypeId);

    public bool TryGetId(NodeChunkHandle chunk, Int3 pos, NodeIdx typeId, out ColumnHandle id)
    {
        id = GetId(chunk, pos, typeId);
        return id != ColumnHandle.Invalid;
    }
    
    public bool TryGetId(NodeVoxel voxel, out ColumnHandle id)
    {
        return TryGetId(voxel.Chunk, voxel.Pos, voxel.TypeId, out id);
    }

    public LayerId GetLayerId(NodeIdx nodeIdx, ColumnHandle column, int layer)
    {
        return GetStorage(nodeIdx).GetLayerId(column, layer);
    }
    
    public NodeIdx NodeTypeToIdx<T>() where T : INode => NodeIdxStorage.Get<T>();
    
    public NodeIdx NetTypeToIdx<T>() where T : INodeNet => NodeIdxStorage.GetNet<T>();
    
    private NodeChunk GetChunk(NodeChunkHandle chunk, NodeIdx typeId) => _chunkMap[chunk.Pos][typeId.Value];
    
    private NodeChunk GetChunk(NodeVoxel voxel) => _chunkMap[voxel.Chunk.Pos][voxel.TypeId.Value];
    
    internal NodeStorage GetStorage(NodeIdx typeId) => _nodeStorages[typeId.Value];
    
    internal NodeStorage GetStorage<T>() where T : INode => _nodeStorages[NodeTypeToIdx<T>().Value];
    
    internal NodeStorage<T> GetStorageTyped<T>() where T : INode => (NodeStorage<T>) _nodeStorages[NodeTypeToIdx<T>().Value];
    
    internal NodeNetStorage GetNetStorage(NodeIdx typeId) => _nodeNetStorages[typeId.Value];
    
    internal NodeNetStorage GetNetStorage<T>() where T : INodeNet => _nodeNetStorages[NetTypeToIdx<T>().Value];
    
    internal NodeNetStorage<T> GetNetStorageTyped<T>() where T : INodeNet => (NodeNetStorage<T>) _nodeNetStorages[NetTypeToIdx<T>().Value];
}
