using Numos.Maths;

namespace Nod3r.API;

/// <summary>
///     Identifies a chunk owned by a <see cref="NodeSimulation"/>.
/// </summary>
public readonly record struct NodeChunkHandle(Int3 Position)
{
    /// <summary>
    /// Implicit operator from <see cref="NodeChunkHandle"/> to <see cref="Int3"/>.
    /// </summary>
    public static implicit operator Int3(NodeChunkHandle handle) => handle.Position;
    
    /// <summary>
    /// Implicit operator from <see cref="Int3"/> to <see cref="NodeChunkHandle"/>.
    /// </summary>
    public static implicit operator NodeChunkHandle(Int3 position) => new(position);
}
