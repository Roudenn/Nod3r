namespace Nod3r.Types;

/// <summary>
/// A config for initializing a <see cref="INodeSolver"/> instance.
/// </summary>
/// <param name="subs">Delegate that specifies which node types and IDs to register.</param>
public sealed class NodeConfig(Action<INodeRegistration> subs)
{
    public readonly Action<INodeRegistration> RegistrationDelegate = subs;
}
