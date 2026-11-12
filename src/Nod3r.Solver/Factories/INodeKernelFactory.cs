using Nod3r.Solver.Kernels;
using Numos.Maths;

namespace Nod3r.Solver.Factories;

internal interface INodeKernelFactory
{
    INodeKernel Create(Int3 dimensions);
}
