using Nod3r.Collections;

namespace Nod3r.Solver.Kernels;

internal sealed partial class NodeKernel<TNode, TNet>
{
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
        var traversed = new HashSet<VoxelHandle>(_newNodes.Count);
        var traversedNets = new HashSet<GenId>();
        var stack = new Stack<VoxelHandle>(_newNodes.Count);
        var cache = new List<VoxelHandle>();
        foreach (var start in _newNodes)
        {
            traversed.Clear();
            stack.Push(start);
            
            while (stack.Count > 0)
            {
                var handle = stack.Pop();
                traversed.Add(handle);
                cache.Clear();
                FindNeighbours(handle, Rule, cache, traversed, traversedNets);
                foreach (var found in cache)
                {
                    stack.Push(found);
                }
            }

            if (traversedNets.Count == 0)
            {
                var net = TNet.CreateNet();
                foreach (var handle in traversed)
                {
                    net.Nodes.Add(GetLayerId(GetId(handle), handle.Layer));
                }
            
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
                var nets = new List<TNet>();
                var biggestId = GenId.Invalid;
                var biggestCount = 0;
                foreach (var id in traversedNets)
                {
                    var net = NodeNetStorage.Get(id);
                    nets.Add(net);
                    if (net.Nodes.Count < biggestCount)
                        continue;
                    
                    biggestId = id;
                    biggestCount = net.Nodes.Count;
                }
                
                var mainNet = NodeNetStorage.Get(biggestId);
                nets.Remove(mainNet); // TODO default equality here is evil
                mainNet.Merge(nets);

                foreach (var net in nets)
                {
                    net.Shutdown();
                }
            }
        }
    }

    private void FindNeighbours(
        VoxelHandle handle,
        NodeRule<TNode> rule,
        List<VoxelHandle> result,
        HashSet<VoxelHandle>? traversed = null,
        HashSet<GenId>? traversedNets = null)
    {
        foreach (var selection in rule.Selections)
        {
            foreach (var relLayer in rule.Layers)
            {
                var layer = (byte) (handle.Layer + relLayer);
                if (!TryGetRelative(handle, selection, layer, out var relative))
                    continue;
            
                if (!TryGetNodeMeta(handle, out var current)
                    || !TryGetNodeMeta(relative, out var neighbor))
                    continue;

                var relPos = FlatArrayHelpers.GetPosition(Dimensions, relative.VoxelIndex)
                             - FlatArrayHelpers.GetPosition(Dimensions, handle.VoxelIndex); // TODO interchunk traversal
                
                if (!rule.Rule.Invoke(new NodeSelectionArgs<TNode>(
                        current.Value.Data,
                        neighbor.Value.Data,
                        relPos,
                        handle.Layer,
                        relative.Layer)))
                    continue;

                if (traversed != null && traversed.Contains(relative))
                    continue;
                
                if (traversedNets != null && neighbor.Value.NetId != GenId.Invalid)
                    traversedNets.Add(neighbor.Value.NetId);
                    
                result.Add(relative);
            }
        }
    }
}
