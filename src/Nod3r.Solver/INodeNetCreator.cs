namespace Nod3r.Solver;

/// <summary>
/// Interface for node networks that allows to statically create new instances of the same type.
/// </summary>
/// <typeparam name="T">Type of node network, must be the same as the implemented type.</typeparam>
public interface INodeNetCreator<out T> where T : INodeNet
{
    /// <summary>
    /// Creates a new <see cref="INodeNet"/> instance.
    /// </summary>
    abstract static T CreateNet();
}
