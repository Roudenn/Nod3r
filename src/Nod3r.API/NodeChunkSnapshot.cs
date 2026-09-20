using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.API;

/// <summary>
/// A read-only snapshot of a node chunk.
/// </summary>
/// <param name="Data"></param>
/// <param name="Dimensions"></param>
/// <param name="Pos"></param>
/// <typeparam name="T"></typeparam>
public readonly record struct NodeChunkSnapshot<T>(
    IReadOnlyList<(T Data, Int3 Pos)> Data,
    Int3 Dimensions,
    Int3 Pos) where T : INode;
