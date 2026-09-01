namespace Nod3r.Types;

public sealed class NodeConfig(Action<INodeRegistration> subs, INodeNetworkFactory factory)
{
    public INodeNetworkFactory Factory = factory;

    public Action<INodeRegistration> RegistrationDelegate = subs;
}
