namespace Nod3r.Collections;

/// <summary>
/// An index that allows to get an element out of a generation data structure.
/// If <see cref="Generation"/> doesn't match with the currently active one in the storage,
/// it will fail to get the data.
/// </summary>
/// <param name="Index">Index in the internal array of a data structure.</param>
/// <param name="Generation">Generation of the referenced array slot.</param>
public readonly record struct GenId(int Index, uint Generation)
{
    /// <summary>
    /// An invalid index.
    /// </summary>
    public static readonly GenId Invalid = new(0, 0);
    
    /// <summary>
    /// Checks whether a specified <see cref="GenId"/> is valid.
    /// </summary>
    public bool IsValid => Generation > 0;
    
    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Index} (G{Generation})";
    }
}
