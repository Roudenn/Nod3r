using Nod3r.Collections;

namespace Nod3r.API;

/// <summary>
///     Identifies one simulation registration.
/// </summary>
/// <remarks>
///     The default value is invalid. Identifiers are meaningful only within the world that issued them.
/// </remarks>
public readonly record struct SimulationId(GenId GenId) : IComparable<SimulationId>
{
    /// <summary>
    ///     Gets whether this value could have been issued by a world registry.
    /// </summary>
    public bool IsValid => GenId.IsValid;

    /// <summary>
    ///     Compares stable slot and generation values without depending on object identity or hash iteration.
    /// </summary>
    /// <param name="other">The identifier to compare.</param>
    /// <returns>
    ///     A negative value when this identifier sorts first, zero when the identifiers match, or a positive value when
    ///     <paramref name="other" /> sorts first.
    /// </returns>
    public int CompareTo(SimulationId other)
    {
        int index = GenId.Index.CompareTo(other.GenId.Index);
        return index != 0 ? index : GenId.Generation.CompareTo(other.GenId.Generation);
    }
}
