using JetBrains.Annotations;
using Nod3r.API.Registration;
using Nod3r.API.Simulations;
using Nod3r.Collections;
using Nod3r.Solver;
using Nod3r.Solver.Chunks;
using Nod3r.Solver.Factories;
using Nod3r.Solver.Kernels;

namespace Nod3r.API;

/// <summary>
/// A collection of <see cref="NodeSimulation"/>s that have the same
/// registered types and configuration.
/// Simulations under the same <see cref="NodeWorld"/> can be linked together with explicit links.
/// </summary>
public sealed class NodeWorld : INodeRegistration
{
    private readonly EnumerableIdStorage<NodeSimulation> _simulations = new();

    private readonly Dictionary<NodeIdx, INodeKernelFactory> _typeKernelFactories = new();
    private readonly Dictionary<NodeIdx, INodeIDKernelFactory> _idKernelFactories = new();
    
    private readonly HashSet<Type> _registeredNodeTypes = [];
    private readonly HashSet<Type> _registeredNodeNetTypes = [];
    
    /// <summary>
    /// All node type indexes that were registered in this solver.
    /// </summary>
    private readonly HashSet<NodeIdx> _registeredIdxs = new();

    /// <summary>
    /// Creates a new <see cref="NodeWorld"/> instance and
    /// automatically does a registration if provided.
    /// </summary>
    /// <param name="registration">
    /// An optional registration delegate that registers
    /// the types available for this <see cref="NodeWorld"/>.
    /// </param>
    public NodeWorld(Action<INodeRegistration>? registration = null)
    {
        registration?.Invoke(this);
    }

    /// <summary>
    /// Creates a new <see cref="NodeSimulation"/> instance under this <see cref="NodeWorld"/>
    /// </summary>
    /// <param name="chunkDepth">Depth of every chunk.</param>
    /// <param name="chunkHeight">Height of every chunk.</param>
    /// <param name="chunkWidth">Width of every chunk.</param>
    /// <returns>A newly created <see cref="NodeSimulation"/>.</returns>
    [PublicAPI]
    public NodeSimulation CreateSimulation(
        int chunkWidth = ChunkConstants.DefaultWidth,
        int chunkHeight = ChunkConstants.DefaultHeight,
        int chunkDepth = ChunkConstants.DefaultDepth)
    {
        var simulation = new NodeSimulation(this, chunkWidth, chunkHeight, chunkDepth);
        return simulation;
    }
    
    /// <summary>
    /// Tries to get a <see cref="NodeSimulation"/> from its <see cref="SimulationId"/>.
    /// </summary>
    [PublicAPI]
    public bool TryGetSimulation(SimulationId id, out NodeSimulation? simulation)
    {
        return _simulations.TryGetValue(id.GenId, out simulation);
    }
    
    /// <summary>
    /// Frees and disposes a <see cref="NodeSimulation"/>.
    /// </summary>
    [PublicAPI]
    public bool DestroySimulation(NodeSimulation simulation)
    {
        return _simulations.TryFree(simulation.Id.GenId);
    }

    /// <summary>
    /// Registers a fully initialized simulation and returns its stable world identifier.
    /// </summary>
    internal SimulationId RegisterSimulation(NodeSimulation simulation)
    {
        foreach (var (typeIdx, factory) in _idKernelFactories)
        {
            simulation.Register(factory.ID, factory, typeIdx);
        }
        
        foreach (var (typeIdx, factory) in _typeKernelFactories)
        {
            simulation.Register(factory, typeIdx);
        }
        
        _simulations.Add(simulation, out var genId);
        return new SimulationId(genId);
    }
    
    /// <inheritdoc/>
    [PublicAPI]
    public void Register<TNode, TNet>(NodeRule<TNode> rule, byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>
    {
        if (_registeredNodeTypes.Contains(typeof(TNode)))
            throw new ArgumentException($"{nameof(INode)} {typeof(TNode).Name} has already been registered in the {nameof(NodeWorld)}!");
            
        if (_registeredNodeNetTypes.Contains(typeof(TNet)))
            throw new ArgumentException($"{nameof(INodeNet)} {typeof(TNet).Name} has already been registered in the {nameof(NodeWorld)}!");
        
        NodeIdxStorage.Register<TNode, TNet>(out var typeIdx);
        if (!_registeredIdxs.Add(typeIdx))
            return;
        
        foreach (var simulation in _simulations)
        {
            simulation.Register<TNode, TNet>(rule, typeIdx, layerCapacity);
        }
        
        _registeredNodeTypes.Add(typeof(TNode));
        _registeredNodeNetTypes.Add(typeof(TNet));
        
        _typeKernelFactories.Add(typeIdx, new NodeKernelFactory<TNode, TNet>(rule));
    }
    
    /// <inheritdoc/>
    [PublicAPI]
    public void RegisterID<TNet>(NodeRule rule, int id)
        where TNet : INodeNet, INodeNetCreator<TNet>
    {
        NodeIdxStorage.Register(id, out var typeIdx);
        if (!_registeredIdxs.Add(typeIdx))
            return;
        
        foreach (var simulation in _simulations)
        {
            simulation.RegisterID<TNet>(id, rule, typeIdx);
        }
        
        _idKernelFactories.Add(typeIdx, new NodeIDKernelFactory<TNet>(id, rule));
    }
}
