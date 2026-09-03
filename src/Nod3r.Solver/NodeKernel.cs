using System.Buffers;
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
    }
    
    /// <summary>
    /// All currently living networks mapped by <see cref="NodeIdx"/>.
    /// </summary>
    private readonly List<NodeNetHandler>[] _nets = [];
    
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

    public void Rebuild()
    {
        SplitNetworks();
        BuildNetworks();
    }

    /// <summary>
    /// Split or delete networks from removed nodes.
    /// </summary>
    private void SplitNetworks()
    {
        // Start from any changed node, flood fill through neighbors,
        // compare to their original networks, split new networks from the parents
    }
    
    /// <summary>
    /// Build new networks from added nodes.
    /// </summary>
    private void BuildNetworks()
    {
        int count = _newNodes.Count;
        var buffer = ArrayPool<NodeVoxel>.Shared.Rent(count);
        _newNodes.CopyTo(buffer, 0);
        
        try 
        {
            // Start from any new node, flood fill until it makes a network,
            // then remove all nodes from the buffer and repeat until every node is flood filled
            var netNodes = new HashSet<NodeVoxel>();
            while (FloodFill(buffer, netNodes, new Stack<NodeVoxel>(), out var start))
            {
                // Create the node network instance.
                var network = _nodeFactories[start.TypeId.Value].Create();

                int typeIdx = start.TypeId.Value;
                
                // First find all already assigned node groups
                // TODO performance
                var networks = new HashSet<NodeNetHandler>();
                foreach (var voxel in netNodes)
                {
                    var genId = GetId(voxel);
                    foreach (var net in _nets[typeIdx])
                    {
                        if (net.Nodes.Contains(net.GetLayerId(this, genId, voxel.Layer)))
                            networks.Add(net);
                    }
                }

                network.Allocate();
                network.Initialize();
                network.Merge(networks);

                _nets[typeIdx].Add(network);
                
                foreach (var net in networks)
                {
                    net.Shutdown();
                    _nets[typeIdx].Remove(net);
                }

                // Skip the added nodes from the buffer since they already have a group
                foreach (var voxel in netNodes)
                {
                    buffer.AsSpan().Replace(voxel, default);
                }
            }
        }
        finally
        {
            ArrayPool<NodeVoxel>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Runs a single iteration of flood fill on a buffer of changed nodes. Returns a constructed network on every iteration.
    /// </summary>
    /// <returns>True if flood fill wasn't done on every node in the <see cref="buffer"/> yet, False if the last node was processed.</returns>
    private bool FloodFill(NodeVoxel[] buffer, HashSet<NodeVoxel> netNodes, Stack<NodeVoxel> stack, out NodeVoxel start)
    {
        start = default;
        foreach (var voxel in buffer)
        {
            if (voxel == default)
                continue;

            start = voxel;
            break;
        }

        if (start == default)
            return true;

        // Start making a node network by using flood fill
        netNodes.Add(start);
        stack.Push(start);

        while (stack.TryPop(out var fillVoxel))
        {
            var neighbours = _ruleFactories[start.TypeId.Value].Create().Evaluate(this, fillVoxel);
            var array = neighbours.ToArray();
            foreach (var voxel in array)
            {
                if (!netNodes.Add(voxel))
                    continue; // An already found node, don't push it again

                stack.Push(voxel);
            }
        }

        return false;
    }

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
    
    public NodeIdx TypeToIdx<T>() where T : INode => NodeIdxStorage.Get<T>();
    
    private NodeChunk GetChunk(NodeChunkHandle chunk, NodeIdx typeId) => _chunkMap[chunk.Pos][typeId.Value];
    
    private NodeChunk GetChunk(NodeVoxel voxel) => _chunkMap[voxel.Chunk.Pos][voxel.TypeId.Value];
}
