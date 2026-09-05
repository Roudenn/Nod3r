namespace Nod3r.Types;

/// <summary>
/// Interface for node networks that allows to statically create new instances of the same type.
/// </summary>
/// <typeparam name="T">Type of node network, must be the same as the implemented type.</typeparam>
public interface INodeNetCreator<out T> where T : INodeNet
{
    abstract static T CreateNet(NodeNetInternal net);
}
