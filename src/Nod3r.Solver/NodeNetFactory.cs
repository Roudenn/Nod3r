using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// A custom object that creates new instances of <see cref="INodeNet"/> types.
/// </summary>
public abstract class NodeNetFactory
{
    public abstract INodeNetInternal Create();
}

public sealed class NodeNetFactory<TNet> : NodeNetFactory
    where TNet : INodeNet, INodeNetCreator<TNet>
{
    public override INodeNetInternal Create()
    {
        return new NodeNet<TNet>();
    }
}
