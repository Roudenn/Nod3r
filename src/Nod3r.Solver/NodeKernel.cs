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
        _nodeIds = new int[NodeIdxStorage.Count];

        int j = 0;
        for (int i = 0; i < NodeIdxStorage.Count; i++)
        {
            if (!_registeredIdxs.Contains(new NodeIdx(i)))
            {
                // This node index wasn't registered in this instance
                _nodeIds[i] = NodeIdx.Invalid.Value;
                continue;
            }
            
            _nodeIds[i] = j;
            j++;
        }

        Initialized = true;
    }

    /// <summary>
    /// Specifies whenever the initialization of the node kernel was completed.
    /// This is marked as true after the end of the object constructor.
    /// </summary>
    public bool Initialized { get; private set; } = false;

    /// <summary>
    /// Registered amount of node types in this solver instance.
    /// </summary>
    public int RegistrationCount { get; private set; } = 0;
    
    /// <summary>
    /// Maps each <see cref="NodeIdx"/> value with a local index
    /// which is used in internal arrays of this kernel instance.
    /// -1 means this type isn't registered.
    /// </summary>
    private int[] _nodeIds;
    
    private NodeStorage[] _nodeStorages = [];
    
    private NodeNetStorage[] _nodeNetStorages = [];

    private readonly HashSet<Type> _registeredNodeTypes = [];
    
    private readonly HashSet<Type> _registeredNodeNetTypes = [];
    
    private readonly HashSet<Type> _registeredNodeRuleTypes = [];
    
    /// <summary>
    /// All node type indexes that were registered in this solver.
    /// </summary>
    private readonly List<NodeIdx> _registeredIdxs = new();
    
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
        return _nets[_nodeIds[typeId.Value]];
    }

    /// <summary>
    /// Gets the <see cref="GenId"/> for <see cref="NodeStorage{T}"/>
    /// from chunk coordinates, node position and <see cref="NodeIdx"/> of the node.
    /// </summary>
    /// <param name="pos">Position inside the chunk.</param>
    /// <param name="chunk">Coordinates of the chunk.</param>
    /// <param name="typeId">Node type index.</param>
    /// <returns><see cref="GenId"/> that can be used in the <see cref="NodeStorage{T}"/> to get the node data.</returns>
    public ColumnHandle GetId(NodeChunkHandle chunk, Int3 pos, NodeIdx typeId) => _chunkMap[chunk.Pos][_nodeIds[typeId.Value]].Chunk[pos];
    
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
    
    private NodeChunk GetChunk(NodeChunkHandle chunk, NodeIdx typeId) => _chunkMap[chunk.Pos][_nodeIds[typeId.Value]];
    
    private NodeChunk GetChunk(NodeVoxel voxel) => _chunkMap[voxel.Chunk.Pos][_nodeIds[voxel.TypeId.Value]];
    
    internal NodeStorage GetStorage(NodeIdx typeId) => _nodeStorages[_nodeIds[typeId.Value]];
    
    internal NodeStorage GetStorage<T>() where T : INode => _nodeStorages[_nodeIds[NodeTypeToIdx<T>().Value]];
    
    internal NodeStorage<T> GetStorageTyped<T>() where T : INode => (NodeStorage<T>) _nodeStorages[_nodeIds[NodeTypeToIdx<T>().Value]];
    
    internal NodeNetStorage GetNetStorage(NodeIdx typeId) => _nodeNetStorages[_nodeIds[typeId.Value]];
    
    internal NodeNetStorage GetNetStorage<T>() where T : INodeNet => _nodeNetStorages[_nodeIds[NetTypeToIdx<T>().Value]];
    
    internal NodeNetStorage<T> GetNetStorageTyped<T>() where T : INodeNet => (NodeNetStorage<T>) _nodeNetStorages[_nodeIds[NetTypeToIdx<T>().Value]];
}
