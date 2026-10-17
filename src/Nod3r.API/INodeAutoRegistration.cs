using Nod3r.Solver;

namespace Nod3r.API;

/// <summary>
/// Allows to automatically register this node type in the 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface INodeAutoRegistration<T> where T : INode
{
    abstract static NodeRule<T> ConnectionRule { get; }
}
