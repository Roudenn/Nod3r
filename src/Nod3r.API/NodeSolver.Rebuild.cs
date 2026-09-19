using System.Collections.Concurrent;
using JetBrains.Annotations;
using Nod3r.Types;

namespace Nod3r.API;

// This partial controls the coordinated solving of all node kernels.
public sealed partial class NodeSolver
{
    /// <summary>
    /// Nodes added since the last solve.
    /// </summary>
    private readonly ConcurrentBag<NodeVoxelHandle> _newNodes = new();

    /// <summary>
    /// Nodes that changed their connection conditions and have to rebuild their group.
    /// </summary>
    private readonly ConcurrentBag<NodeVoxelHandle> _changedNodes = new();
    
    /// <summary>
    /// Chunks that got modified since the last solve.
    /// </summary>
    private readonly HashSet<NodeChunkHandle> _changedChunks = new();
    
    /// <summary>
    /// General function that updates all node networks in this solver instance.
    /// </summary>
    [PublicAPI]
    public void Rebuild()
    {
        // TODO
    }
}
