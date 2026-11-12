using Numos.Maths;

namespace Nod3r.Solver;

internal interface INodeKernelFactory
{
    INodeKernel Create(Int3 dimensions);
}
