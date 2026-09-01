using Nod3r.Collections;

namespace Nod3r.Types;

/// <summary>
/// Represents an address to get a node with specific type from a <see cref="NodeStorage{T}"/>.
/// </summary>
public readonly record struct NodeAddr(GenId Id, int Layer);
