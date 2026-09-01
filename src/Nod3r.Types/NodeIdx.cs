namespace Nod3r.Types;

/// <summary>
/// Represents a registered node type.
/// </summary>
/// <remarks>
/// This type is guaranteed to be valid only for each specific solver instance,
/// since each solver is initialized independently.
/// It is safe to reuse this index for other solvers only if their <see cref="NodeConfig"/> is the same.
/// </remarks>
public readonly record struct NodeIdx(int Value);
