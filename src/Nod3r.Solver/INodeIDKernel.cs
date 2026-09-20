using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

internal interface INodeIDKernel : INodeKernel
{
    void SetNode(NodeVoxelHandle voxel);
    
    void GetChunkData(NodeChunkHandle chunk, HashSet<Int3> list, out Int3 dimensions);
}
