namespace Nod3r.Types;

public sealed class NodeConfig(Action<INodeRegistration> subs)
{
    public readonly Action<INodeRegistration> RegistrationDelegate = subs;
}
