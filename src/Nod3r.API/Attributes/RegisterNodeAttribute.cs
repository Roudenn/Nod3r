using Nod3r.Solver;

namespace Nod3r.API.Attributes;

/// <summary>
/// Automatically registers an <see cref="INode"/> in all <see cref="NodeWorld"/>s created with their default constructor.
/// </summary>
/// <param name="category">
/// Optional category to specifically register
/// specific nodes during <see cref="NodeWorld"/> creation.
/// </param>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class RegisterNodeAttribute(string? category = null) : Attribute
{
    /// <summary>
    /// Category of the node registration.
    /// </summary>
    public readonly string? Category = category;
}
