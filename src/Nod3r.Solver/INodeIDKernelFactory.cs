using Numos.Maths;

namespace Nod3r.Solver;

internal interface INodeIDKernelFactory
{
    int ID { get; }
    
    INodeIDKernel Create(Int3 dimensions);
}
