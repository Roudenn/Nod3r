using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// Base class for node groups that control a certain node type.
/// </summary>
/// <typeparam name="T">The controlled node type.</typeparam>
public abstract class NodeNet<T> : INodeNet where T : INode
{
    public HashSet<LayerId> Nodes { get; protected set; } = new();
    
    public virtual void Initialize() { }

    public virtual void Shutdown() { }

    public virtual void Merge(IReadOnlySet<INodeNet> nets) { }

    public virtual void Split(INodeNet parent) { }

    public LayerId GetStackId(ColumnHandle idx, int layer)
    {
        return NodeStorage<T>.GetLayerId(idx, layer);
    }
}
