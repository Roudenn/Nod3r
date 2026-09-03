using System.Buffers;
using Nod3r.Types;

namespace Nod3r.Solver;

// Contains the main solver methods that build node networks.
internal sealed partial class NodeKernel
{
    /// <summary>
    /// Rebuilds all node networks, starting from added, removed and changed nodes.
    /// </summary>
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
                var networks = new HashSet<NodeNetInternal>();
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
}
