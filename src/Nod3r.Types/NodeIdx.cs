namespace Nod3r.Types;

/// <summary>
/// Represents a registered node type.
/// </summary>
/// <remarks>
/// The value is specific for every solver instance, so it should only be used internally while doing inner solver calculations.
/// </remarks>
public readonly record struct NodeIdx(int Value)
{
    public readonly static NodeIdx Invalid = new(-1);
    
    public bool IsValid => Value >= 0;
}
