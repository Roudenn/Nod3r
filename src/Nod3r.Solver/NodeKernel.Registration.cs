using Nod3r.Types;

namespace Nod3r.Solver;

internal sealed partial class NodeKernel
{
    public void Register<TNode, TNet, TRule>(byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet, INodeNetCreator<TNet>
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
        EnsureArrayCapacity(ref _nodeStorages, RegistrationCount + 1);
        EnsureArrayCapacity(ref _nodeNetStorages, RegistrationCount + 1);
        EnsureArrayCapacity(ref _nets, RegistrationCount + 1);
        
        _nodeStorages[RegistrationCount] = new NodeStorage<TNode>();
        _nodeNetStorages[RegistrationCount] = new NodeNetStorage<TNet>();
        
        _nodeStorages[RegistrationCount].EnsureLayerCapacity(layerCapacity);
        RegistrationCount++;
        
        _ruleFactories.Add(new NodeRuleFactory<TNode, TRule>());
        _nodeFactories.Add(new NodeNetFactory<TNet>());
        
        _registeredNodeTypes.Add(typeof(TNode));
        _registeredNodeNetTypes.Add(typeof(TNet));
        _registeredNodeRuleTypes.Add(typeof(TRule));
    }

    private static void EnsureArrayCapacity<T>(ref T[] array, int capacity)
    {
        if ((uint) capacity < (uint) array.Length)
            return;
        
        if (array.Length < capacity || array.Length < 4)
            Array.Resize(ref array, Math.Max(Math.Max(array.Length, 2) * 2, capacity));
    }
}
