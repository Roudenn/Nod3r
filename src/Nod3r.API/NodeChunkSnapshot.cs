using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.API;

/// <summary>
/// A read-only snapshot of a type node chunk.
/// </summary>
/// <param name="Data"></param>
/// <param name="Dimensions"></param>
/// <param name="Pos"></param>
/// <typeparam name="T"></typeparam>
public readonly record struct NodeChunkSnapshot<T>(
    IReadOnlyList<(T Data, Int3 Pos)> Data,
    Int3 Dimensions,
    Int3 Pos) where T : INode;

/// <summary>
/// A read-only snapshot of an ID node chunk.
/// </summary>
public readonly record struct NodeChunkSnapshot(
    int ID,
    IReadOnlySet<Int3> Data,
    Int3 Dimensions,
    Int3 Pos);
