using Nod3r.Solver;

namespace Nod3r.API.Attributes;

/// <summary>
/// Automatically implements interfaces for <see cref="INodeNet"/>
/// to reduce boilerplate from implementing multiple empty methods.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class AutoNodeNetworkAttribute : Attribute;
