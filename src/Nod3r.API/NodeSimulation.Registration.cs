using Nod3r.Collections;
using Nod3r.Solver;

namespace Nod3r.API;

public sealed partial class NodeSimulation
{
    private readonly HashSet<Type> _registeredNodeTypes = [];
    
    private readonly HashSet<Type> _registeredNodeNetTypes = [];
    
    public void Register<TNode, TNet>(NodeRule<TNode> rule, byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>
    {
        if (_registeredNodeTypes.Contains(typeof(TNode)))
            throw new ArgumentException($"Node {typeof(TNode).Name} has already been registered in the kernel!");
            
        if (_registeredNodeNetTypes.Contains(typeof(TNet)))
            throw new ArgumentException($"Node network {typeof(TNet).Name} has already been registered in the kernel!");
        
        NodeIdxStorage.Register<TNode, TNet>(out var typeIdx);
        _registeredIdxs.Add(typeIdx);

        // Since we can't get the type parameters after the registration method was completed,
        // we have to initialize the storages right now, without knowing the total amount of registrations.
        // This allows to register node types dynamically without having to call a separate method.
        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count);

        _kernels[RegistrationCount] = new NodeKernel<TNode, TNet>(Dimensions);
        _idxes[typeIdx.Value] = RegistrationCount;
        
        RegistrationCount++;
        
        _registeredNodeTypes.Add(typeof(TNode));
        _registeredNodeNetTypes.Add(typeof(TNet));
    }

    public void RegisterID<TNet>(NodeRule rule, int id)
        where TNet : INodeNet, INodeNetCreator<TNet>
    {
        NodeIdxStorage.Register(id, out var typeIdx);
        _registeredIdxs.Add(typeIdx);

        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count, NodeIdx.Invalid.Value);
        
        _kernels[RegistrationCount] = new NodeIDKernel<TNet>(id, Dimensions);
        _idxes[typeIdx.Value] = RegistrationCount;
        
        RegistrationCount++;
    }
}
