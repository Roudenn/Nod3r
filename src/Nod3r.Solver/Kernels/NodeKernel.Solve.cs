using Numos.Maths;

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
                FindNeighbours(handle, Rule, cache, traversed);
                foreach (var found in cache)
                {
                    stack.Push(found);
                }
            }

            // TODO detect that a node is already connected to a network and merge the new nodes to it instead
            var net = TNet.CreateNet();
            foreach (var handle in traversed)
            {
                net.Nodes.Add(GetLayerId(GetId(handle), handle.Layer));
            }
            
            net.Initialize();
            var id = NodeNetStorage.Add(net);
            Nets.Add(id);
        }
    }

    private void FindNeighbours(VoxelHandle handle, NodeRule<TNode> rule, List<VoxelHandle> result, HashSet<VoxelHandle>? traversed = null)
    {
        foreach (var selection in rule.Selections)
        {
            foreach (var relLayer in rule.Layers)
            {
                var layer = (byte) (handle.Layer + relLayer);
                if (!TryGetRelative(handle, selection, layer, out var relative))
                    continue;
            
                if (!TryGetNode(handle, out var current)
                    || !TryGetNode(relative, out var neighbor))
                    continue;

                var relPos = FlatArrayHelpers.GetPosition(Dimensions, relative.VoxelIndex)
                             - FlatArrayHelpers.GetPosition(Dimensions, handle.VoxelIndex); // TODO interchunk traversal
                if (!rule.Rule.Invoke(new NodeSelectionArgs<TNode>(current, neighbor, relPos, handle.Layer, relative.Layer)))
                    continue;
            
                if (traversed is null || !traversed.Contains(relative))
                    result.Add(relative);
            }
        }
    }
}
