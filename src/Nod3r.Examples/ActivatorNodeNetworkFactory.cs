using Nod3r.Solver;
using Nod3r.Types;

namespace Nod3r.Examples;

/// <summary>
/// Simple implementation of <see cref="INodeNetworkFactory"/>
/// that uses the <see cref="Activator"/> to create node net instances.
/// Doesn't make any type checks before creating the instance.
/// </summary>
public sealed class ActivatorNodeNetworkFactory : INodeNetworkFactory
{
    public INodeNet Create(NodeIdx node)
    {
        return (INodeNet)Activator.CreateInstance(NodeIdxStorage.GetNetworkType(node))!;
    }
}
