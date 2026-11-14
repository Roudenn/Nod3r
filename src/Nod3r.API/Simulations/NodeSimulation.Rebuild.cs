using CommunityToolkit.HighPerformance.Helpers;
using JetBrains.Annotations;
using Nod3r.Solver.Kernels;

namespace Nod3r.API.Simulations;

// This partial controls the coordinated solving of all node kernels.
public sealed partial class NodeSimulation
{
    /// <summary>
    /// General function that updates all node networks in this solver instance.
    /// </summary>
    [PublicAPI]
    public void Rebuild()
    {
        // TODO:
        // Currently kernels can't interact with each other.
        // This may be an issue in the future when more functionality is added.
        // If that ever happens, this code below should also take into account kernel update order.
        ParallelHelper.For(0, RegistrationCount, new RebuildAction(_kernels));
    }

    private readonly struct RebuildAction(INodeKernel[] kernels) : IAction
    {
        public void Invoke(int i)
        {
            kernels[i].Rebuild();
        }
    }
}
