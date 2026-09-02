using System.Collections.Immutable;
using Numos.Maths;

namespace Nod3r.API;

public static class Int3Helpers
{
    public readonly static ImmutableArray<Int3> CardinalOffsets =
    [
        Int3.PosX, Int3.NegX, Int3.PosY, Int3.NegY, Int3.PosZ, Int3.NegZ
    ];
}
