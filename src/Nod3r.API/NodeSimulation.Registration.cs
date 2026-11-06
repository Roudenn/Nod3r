using Nod3r.Collections;
using Nod3r.Solver;

namespace Nod3r.API;

public sealed partial class NodeSimulation
{
    internal void Register<TNode, TNet>(NodeRule<TNode> rule, byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>
    {
        NodeIdxStorage.Register<TNode, TNet>(out var typeIdx);
        if (!_registeredIdxs.Add(typeIdx))
            return;

        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count);

        _kernels[RegistrationCount] = new NodeKernel<TNode, TNet>(Dimensions);
        _idxes[typeIdx.Value] = RegistrationCount;
        
        RegistrationCount++;
    }

    internal void RegisterID<TNet>(NodeRule rule, int id)
        where TNet : INodeNet, INodeNetCreator<TNet>
    {
        NodeIdxStorage.Register(id, out var typeIdx);
        if (!_registeredIdxs.Add(typeIdx))
            return;

        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count, NodeIdx.Invalid.Value);
        
        _kernels[RegistrationCount] = new NodeIDKernel<TNet>(id, Dimensions);
        _idxes[typeIdx.Value] = RegistrationCount;
        
        RegistrationCount++;
    }
}
