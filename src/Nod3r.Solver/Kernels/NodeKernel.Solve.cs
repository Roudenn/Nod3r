using Nod3r.Collections;

namespace Nod3r.Solver.Kernels;

internal sealed partial class NodeKernel<TNode, TNet>
{
    private readonly Dictionary<GenId, TNet> _netsCache = new();
    
    public void Rebuild()
    {
        BuildNetworks();
    }

    /// <summary>
    /// Connects all new nodes to existing network
    /// and creates new networks for new isolated groups.
    /// </summary>
    private void BuildNetworks()
    {
        var visited = new HashSet<VoxelHandle>(_newNodes.Count);
        var traversedNets = new HashSet<GenId>();
        var stack = new Stack<VoxelHandle>(_newNodes.Count);
        var cache = new List<Candidate>();
        var netNodeIds = new HashSet<LayerId>();
        foreach (var start in _newNodes)
        {
            if (visited.Contains(start))
                continue; // This node was already included to a network
            
            stack.Push(start);

            traversedNets.Clear();
            
            while (stack.Count > 0)
            {
                var handle = stack.Pop();
                cache.Clear();
                FindNeighbours(handle, Rule, cache);
                foreach (var found in cache)
                {
                    if (traversedNets.Contains(found.NetId))
                        continue;
                
                    if (!visited.Add(found.Handle))
                        continue; // Already visited
                    
                    // If it's a part of a network then this branch should be stopped.
                    if (found.NetId != GenId.Invalid)
                    {
                        traversedNets.Add(found.NetId);
                        continue;
                    }
                    
                    stack.Push(found.Handle);
                    netNodeIds.Add(GetLayerId(found.Handle));
                }
            }

            if (traversedNets.Count == 0)
            {
                // All nodes are new in this group, create a new network for them
                var net = TNet.CreateNet();
                net.Nodes.UnionWith(netNodeIds);
                net.Initialize();
                
                var id = NodeNetStorage.Add(net);
                foreach (var layerId in net.Nodes)
                {
                    NodeStorage.Set(layerId, NodeStorage.Get(layerId) with { NetId = id });
                }
                Nets.Add(id);
            }
            else
            {
                _netsCache.Clear();
                _netsCache.EnsureCapacity(traversedNets.Count);
                var biggestId = GenId.Invalid;
                var biggestCount = 0;
                foreach (var id in traversedNets)
                {
                    var net = NodeNetStorage.Get(id);
                    _netsCache.Add(id, net);
                    if (net.Nodes.Count < biggestCount)
                        continue;
                    
                    biggestId = id;
                    biggestCount = net.Nodes.Count;
                }
                
                var mainNet = NodeNetStorage.Get(biggestId);
                _netsCache.Remove(biggestId);
                mainNet.Merge(_netsCache.Values.ToList());
                mainNet.Nodes.UnionWith(netNodeIds);

                foreach (var net in _netsCache.Values)
                {
                    net.Shutdown();
                }
            }
        }
    }

    private void FindNeighbours(
        VoxelHandle handle,
        NodeRule<TNode> rule,
        List<Candidate> result)
    {
        var curPosition = FlatArrayHelpers.GetPosition(Dimensions, handle.VoxelIndex);
        
        if (!TryGetNodeMeta(handle, out var current))
            return;
        
        foreach (var selection in rule.Selections)
        {
            foreach (var relLayer in rule.Layers)
            {
                var layer = (byte) (handle.Layer + relLayer);
                if (!TryGetRelative(handle, selection, layer, out var relative))
                    continue;
            
                if (!TryGetNodeMeta(relative, out var neighbor))
                    continue;

                // TODO interchunk traversal
                var relPos = FlatArrayHelpers.GetPosition(Dimensions, relative.VoxelIndex) - curPosition;

                var args = new NodeSelectionArgs<TNode>(
                    current.Value.Data,
                    neighbor.Value.Data,
                    relPos,
                    handle.Layer,
                    relative.Layer);
                
                if (!rule.Rule.Invoke(args))
                    continue;
                
                result.Add(new Candidate(relative, neighbor.Value.NetId));
            }
        }
    }
    
    private record struct Candidate(VoxelHandle Handle, GenId NetId);
}
