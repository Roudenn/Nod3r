using Nod3r.Collections;

namespace Nod3r.Solver.Kernels;

internal sealed partial class NodeIDKernel<TNet>
{
    private readonly ObjectPool<Group> _groupPool = new(() => new Group());
    private readonly List<Group> _groupCache = new();
    private readonly Dictionary<GenId, TNet> _netsCache = new();
    private readonly HashSet<VoxelHandle> _visited = new();
    private readonly Stack<VoxelHandle> _stack = new();
    private readonly List<Candidate> _cache = new();
    private TNet[] _netsArrayCache = [];

    public void Rebuild()
    {
        BuildNetworks();
        _newNodes.Clear();
    }

    /// <summary>
    /// Connects all new nodes to existing network
    /// and creates new networks for new isolated groups.
    /// </summary>
    private void BuildNetworks()
    {
        _visited.Clear();
        _visited.EnsureCapacity(_newNodes.Count);
        _stack.Clear();
        _stack.EnsureCapacity(_newNodes.Count);
        _cache.Clear();

        foreach (var start in _newNodes)
        {
            if (_visited.Contains(start))
                continue; // This node was already included to a network

            var group = _groupPool.Get();
            _stack.Push(start);
            group.TraversedNets.Clear();

            while (_stack.Count > 0)
            {
                var handle = _stack.Pop();
                _cache.Clear();
                FindNeighbours(handle, _rule, _cache);
                foreach (var found in _cache)
                {
                    if (group.TraversedNets.Contains(found.NetId))
                        continue;

                    if (!_visited.Add(found.Handle))
                        continue; // Already visited

                    // If it's a part of a network then this branch should be stopped.
                    if (found.NetId != GenId.Invalid)
                    {
                        group.TraversedNets.Add(found.NetId);
                        continue;
                    }

                    _stack.Push(found.Handle);
                }
            }

            _groupCache.Add(group);
        }

        foreach (var group in _groupCache)
        {
            if (group.TraversedNets.Count == 0)
            {
                // All nodes are new in this group, create a new network for them
                var net = TNet.CreateNet();
                net.Initialize();

                var id = NodeNetStorage.Add(net);
                Nets.Add(id);
            }
            else
            {
                _netsCache.Clear();
                _netsCache.EnsureCapacity(group.TraversedNets.Count);
                var biggestId = GenId.Invalid;
                var biggestCount = 0;
                foreach (var id in group.TraversedNets)
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

                ArrayHelpers.EnsureCapacity(ref _netsArrayCache, _netsCache.Count);
                _netsCache.Values.CopyTo(_netsArrayCache, 0);
                mainNet.Merge(_netsArrayCache.AsSpan(0, _netsCache.Count));

                foreach (var net in _netsCache.Values)
                {
                    net.Shutdown();
                }
            }

            group.TraversedNets.Clear();
            _groupPool.Return(group);
        }
    }

    private void FindNeighbours(
        VoxelHandle handle,
        NodeRule rule,
        List<Candidate> result)
    {
        foreach (var selection in rule.Selections)
        {
            if (!TryGetRelativeCandidate(handle, selection, out var relative))
                continue;

            result.Add(relative);
        }
    }

    private record struct Candidate(VoxelHandle Handle, GenId NetId);

    private readonly struct Group()
    {
        public readonly HashSet<GenId> TraversedNets = new();
    }
}
