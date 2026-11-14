using Nod3r.Solver;

namespace Nod3r.API.Registration;

/// <summary>
/// Represents a registered node in a program.
/// Can represent either a Typed <see cref="INode"/> or an ID Node.
/// </summary>
public readonly record struct NodeIdx(int Value)
{
    /// <summary>
    /// An invalid index.
    /// </summary>
    public static readonly NodeIdx Invalid = new(-1);
    
    /// <summary>
    /// Checks whether this index is valid.
    /// </summary>
    public bool IsValid => Value >= 0;
}
