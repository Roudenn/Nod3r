using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.API;

/// <summary>
/// A read-only snapshot of a type node chunk.
/// </summary>
/// <param name="Data">
/// Data about each node in the snapshot. If multiple layers are supporte
/// multiple nodes may share the same position.
/// </param>
/// <param name="Dimensions">Dimensions of the chunk.</param>
/// <param name="Handle">Chunk handle of the snapshot.</param>
/// <typeparam name="T">Type of node stored in this snapshot</typeparam>
public readonly record struct NodeChunkSnapshot<T>(
    IReadOnlyList<(T Data, Int3 Pos)> Data,
    Int3 Dimensions,
    NodeChunkHandle Handle) where T : INode;

/// <summary>
/// A read-only snapshot of an ID node chunk.
/// </summary>
/// <param name="ID">Node ID of the snapshot.</param>
/// <param name="Data">Position of every living node.</param>
/// <param name="Dimensions">Dimensions of the chunk.</param>
/// <param name="Handle">Chunk handle of the snapshot.</param>
public readonly record struct NodeChunkSnapshot(
    int ID,
    IReadOnlySet<Int3> Data,
    Int3 Dimensions,
    NodeChunkHandle Handle);
