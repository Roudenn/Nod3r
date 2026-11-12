using Nod3r.API.Registration;
using Nod3r.Collections;
using Nod3r.Solver;

namespace Nod3r.API.Simulations;

public sealed partial class NodeSimulation
{
    internal void Register<TNode, TNet>(NodeRule<TNode> rule, NodeIdx typeIdx, byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>
    {
        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count);

        _kernels[RegistrationCount] = new NodeKernel<TNode, TNet>(Dimensions);
        _idxes[typeIdx.Value] = RegistrationCount;
        
        RegistrationCount++;
    }

    internal void RegisterID<TNet>(int id, NodeRule rule, NodeIdx typeIdx)
        where TNet : INodeNet, INodeNetCreator<TNet>
    {
        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count, NodeIdx.Invalid.Value);
        
        _kernels[RegistrationCount] = new NodeIDKernel<TNet>(id, rule, Dimensions);
        _idxes[typeIdx.Value] = RegistrationCount;
        
        RegistrationCount++;
    }

    internal void Register(INodeKernelFactory factory, NodeIdx typeIdx, byte layerCapacity = 1)
    {
        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count);

        _kernels[RegistrationCount] = factory.Create(Dimensions);
        _idxes[typeIdx.Value] = RegistrationCount;
        
        RegistrationCount++;
    }
    
    internal void Register(int id, INodeIDKernelFactory factory, NodeIdx typeIdx)
    {
        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count);

        _kernels[RegistrationCount] = factory.Create(Dimensions);
        _idxes[typeIdx.Value] = RegistrationCount;
        
        RegistrationCount++;
    }
}
