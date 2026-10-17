using Nod3r.Solver;

namespace Nod3r.API;

/// <summary>
/// Represents a registered node in a program.
/// Can represent either a Typed <see cref="INode"/> or an ID Node.
/// </summary>
public readonly record struct NodeIdx(int Value)
{
    public readonly static NodeIdx Invalid = new(-1);
    
    public bool IsValid => Value >= 0;
}
