using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.Solver;

public abstract class NodeNet<T> : INodeNet where T : INode
{
    public HashSet<StackGenId> Nodes { get; protected set; } = new();
    
    public virtual void Initialize() { }

    public virtual void Shutdown() { }

    public virtual void Merge(IReadOnlySet<INodeNet> nets) { }

    public virtual void Split(INodeNet parent) { }

    public StackGenId GetStackId(GenIdx idx, int layer)
    {
        return NodeStorage<T>.GetStackId(idx, layer);
    }
}
