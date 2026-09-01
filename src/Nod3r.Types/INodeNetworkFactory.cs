namespace Nod3r.Types;

/// <summary>
/// A custom object that creates new instances of <see cref="INodeNet"/> types.
/// </summary>
/// <remarks>
/// This exists for safety reasons, so it is possible to
/// implement custom checks to restrict creation of unallowed types.
/// </remarks>
public interface INodeNetworkFactory
{
    INodeNet Create(NodeIdx node);
}
