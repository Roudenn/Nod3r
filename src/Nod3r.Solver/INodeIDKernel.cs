using Nod3r.Types;

namespace Nod3r.Solver;

internal interface INodeIDKernel : INodeKernel
{
    void SetNode(NodeVoxelHandle voxel);
}
