using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// A custom object that creates new instances of <see cref="INodeNet"/> types.
/// </summary>
public abstract class NodeNetFactory
{
    public abstract NodeNetInternal Create();
}

public sealed class NodeNetFactory<TNode, TNet> : NodeNetFactory
    where TNode : INode
    where TNet : INodeNet, INodeNetCreator<TNet>
{
    public override NodeNetInternal Create()
    {
        return new NodeNet<TNode, TNet>();
    }
}
