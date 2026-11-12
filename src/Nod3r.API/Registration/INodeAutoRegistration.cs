using Nod3r.Solver;

namespace Nod3r.API.Registration;

/// <summary>
/// Allows to automatically register this <see cref="INode"/> type in
/// a <see cref="NodeWorld"/> by specifying a <see cref="NodeRule{T}"/>.
/// </summary>
/// <typeparam name="T">
/// Type of the node that is registered automatically by this type.
/// </typeparam>
public interface INodeAutoRegistration<T> where T : INode
{
    abstract static NodeRule<T> ConnectionRule { get; }
}
