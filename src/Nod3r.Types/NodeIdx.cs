namespace Nod3r.Types;

/// <summary>
/// Represents a registered node type. The value is shared across all solver instances.
/// </summary>
public readonly record struct NodeIdx(int Value)
{
    public static readonly NodeIdx Invalid = new(-1);
}
