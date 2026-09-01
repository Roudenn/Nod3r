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
        _nodeFactory = config.Factory;
    }
    
    /// <summary>
    /// <see cref="INodeRule"/>s mapped by the <see cref="NodeIdx"/>.
    /// </summary>
    private readonly List<INodeRule> _rules = new();
    
    /// <summary>
    /// All currently living networks mapped by <see cref="NodeIdx"/>.
    /// </summary>
    private readonly List<INodeNet>[] _nets = [];
    
    /// <summary>
    /// A factory that creates <see cref="INodeNet"/> objects from <see cref="NodeIdx"/>.
    /// </summary>
    private readonly INodeNetworkFactory _nodeFactory;
    
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
        // 
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
            while (true)
            {
                NodeVoxel start = default;
                for (int i = 0; i < count; i++)
                {
                    var voxel = buffer[i];
                    if (voxel == default)
                        continue;

                    start = voxel;
                    break;
                }

                if (start == default)
                    break;

                // Start making a node network by using flood fill
                var netNodes = new HashSet<NodeVoxel> { start };
                var stack = new Stack<NodeVoxel>();
                stack.Push(start);
                int typeId = start.TypeId.Value;

                while (stack.TryPop(out var fillVoxel))
                {
                    var neighbours = _rules[start.TypeId.Value].Evaluate(this, fillVoxel);
                    var array = neighbours.ToArray();
                    foreach (var voxel in array)
                    {
                        if (!netNodes.Add(voxel))
                            continue; // An already found node, don't push it again

                        stack.Push(voxel);
                    }
                }

                // Create the node network instance.

                var network = _nodeFactory.Create(start.TypeId);

                // First find all already assigned node groups
                // TODO performance
                var networks = new HashSet<INodeNet>();
                foreach (var voxel in netNodes)
                {
                    var genId = GetId(voxel);
                    foreach (var net in _nets[typeId])
                    {
                        if (net.Nodes.Contains(net.GetStackId(genId, voxel.Layer)))
                            networks.Add(net);
                    }
                }

                network.Merge(networks);

                _nets[typeId].Add(network);
                
                foreach (var net in networks)
                {
                    _nets[typeId].Remove(net);
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

    public void SetNode<T>(ref T node, NodeVoxel voxel) where T : INode
    {
        var chunk = GetChunk(voxel);
        var oldGenId = chunk.Chunk[voxel.Pos];
        if (oldGenId.IsValid())
            NodeStorage<T>.Free(oldGenId, voxel.Layer);
        
        NodeStorage<T>.Allocate(voxel.Layer, out var slot) = node;
        chunk.Chunk[voxel.Pos] = slot.Idx;
        _newNodes.Add(voxel);
        _changedChunks.Add(voxel.Chunk);
    }

    /// <summary>
    /// Removes a node voxel from the chunk map.
    /// </summary>
    /// <param name="voxel">Node voxel to remove.</param>
    /// <typeparam name="T">Type of the node to remove.</typeparam>
    public void RemoveNode<T>(NodeVoxel voxel) where T : INode
    {
        NodeStorage<T>.Free(GetId(voxel), voxel.Layer);
        GetChunk(voxel).Chunk[voxel.Pos] = GenIdx.Invalid;
        _changedChunks.Add(voxel.Chunk);
        var neighbors = _rules[voxel.TypeId.Value].Evaluate(this, voxel);
        foreach (var nearVoxel in neighbors)
        {
            _changedNodes.Add(nearVoxel);
        }
    }
    
    /// <summary>
    /// Marks a node voxel as changed, which will force the parent network to update.
    /// Call this method when <see cref="INodeRule"/> have potentially changed.
    /// </summary>
    /// <param name="voxel"></param>
    public void DirtyNode(NodeVoxel voxel)
    {
        
    }

    /// <summary>
    /// Gets the <see cref="GenId"/> for <see cref="NodeStorage{T}"/>
    /// from chunk coordinates, node position and <see cref="NodeIdx"/> of the node.
    /// </summary>
    /// <param name="pos">Position inside the chunk.</param>
    /// <param name="chunk">Coordinates of the chunk.</param>
    /// <param name="typeId">Node type index.</param>
    /// <returns><see cref="GenId"/> that can be used in the <see cref="NodeStorage{T}"/> to get the node data.</returns>
    public GenIdx GetId(NodeChunkHandle chunk, Int3 pos, NodeIdx typeId) => _chunkMap[chunk.Pos][typeId.Value].Chunk[pos];
    
    /// <summary>
    /// Gets the <see cref="GenId"/> for <see cref="NodeStorage{T}"/> from a <see cref="NodeVoxel"/>.
    /// </summary>
    /// <param name="voxel">The target node voxel.</param>
    /// <returns><see cref="GenId"/> that can be used in the <see cref="NodeStorage{T}"/> to get the node data.</returns>
    public GenIdx GetId(NodeVoxel voxel) => GetId(voxel.Chunk, voxel.Pos, voxel.TypeId);
    
    private NodeChunk GetChunk(NodeChunkHandle chunk, NodeIdx typeId) => _chunkMap[chunk.Pos][typeId.Value];
    
    private NodeChunk GetChunk(NodeVoxel voxel) => _chunkMap[voxel.Chunk.Pos][voxel.TypeId.Value];
}
