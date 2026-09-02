namespace Nod3r.Types;

public sealed class NodeConfig(Action<INodeRegistration> subs)
{
    public Action<INodeRegistration> RegistrationDelegate = subs;
}
