using Numos.Maths;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Nod3r.API;

// TODO move to Numos.Maths
public static class Int3Helpers
{
    public static readonly Int3 Zero = new Int3(0, 0, 0);
    
    public static readonly Int3[] CardinalOffsets =
    [
        Int3.PosX, Int3.NegX, Int3.PosY, Int3.NegY, Int3.PosZ, Int3.NegZ
    ];
}
