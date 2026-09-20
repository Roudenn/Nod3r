using Nod3r.Collections;
using Nod3r.Solver;
using Nod3r.Types;

namespace Nod3r.API;

public sealed partial class NodeSolver
{
    private readonly HashSet<Type> _registeredNodeTypes = [];
    
    private readonly HashSet<Type> _registeredNodeNetTypes = [];
    
    private readonly HashSet<Type> _registeredNodeRuleTypes = [];
    
    public void Register<TNode, TNet, TRule>(byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>
        where TRule : INodeRule<TNode>, INodeRuleCreator<TRule>
    {
        if (_registeredNodeTypes.Contains(typeof(TNode)))
            throw new ArgumentException($"Node {typeof(TNode).Name} has already been registered in the kernel!");
            
        if (_registeredNodeNetTypes.Contains(typeof(TNet)))
            throw new ArgumentException($"Node network {typeof(TNet).Name} has already been registered in the kernel!");
                
        if (_registeredNodeRuleTypes.Contains(typeof(TRule)))
            throw new ArgumentException($"Node rule {typeof(TRule).Name} has already been registered in the kernel!");
        
        NodeIdxStorage.Register<TNode, TNet>(out var typeIdx);
        _registeredIdxs.Add(typeIdx);

        // Since we can't get the type parameters after the registration method was completed,
        // we have to initialize the storages right now, without knowing the total amount of registrations.
        // This allows to register node types dynamically without having to call a separate method.
        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count);

        _kernels[RegistrationCount] = new NodeKernel<TNode, TNet, TRule>(this);
        _idxes[typeIdx.Value] = RegistrationCount;
        
        RegistrationCount++;
        
        _registeredNodeTypes.Add(typeof(TNode));
        _registeredNodeNetTypes.Add(typeof(TNet));
        _registeredNodeRuleTypes.Add(typeof(TRule));
    }

    public void RegisterID<TNet, TRule>(int id, out NodeIdx registered)
        where TNet : INodeNet, INodeNetCreator<TNet>
        where TRule : INodeRule, INodeRuleCreator<TRule>
    {
        NodeIdxStorage.Register<TNet>(id, out registered);
        _registeredIdxs.Add(registered);

        ArrayHelpers.EnsureCapacity(ref _kernels, RegistrationCount);
        ArrayHelpers.EnsureCapacity(ref _idxes, NodeIdxStorage.Count, NodeIdx.Invalid.Value);
        
        _kernels[RegistrationCount] = new NodeIDKernel<TNet, TRule>(this, id);
        _idxes[registered.Value] = RegistrationCount;
        
        RegistrationCount++;
    }
}
