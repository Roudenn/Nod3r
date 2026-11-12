using Nod3r.Solver.Kernels;
using Numos.Maths;

namespace Nod3r.Solver.Factories;

internal interface INodeIDKernelFactory
{
    int ID { get; }
    
    INodeIDKernel Create(Int3 dimensions);
}
