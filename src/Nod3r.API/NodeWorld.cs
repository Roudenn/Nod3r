using JetBrains.Annotations;
using Nod3r.Collections;
using Nod3r.Solver;

namespace Nod3r.API;

public sealed class NodeWorld : INodeRegistration
{
    private readonly GenIdStorage<NodeSimulation> _simulations = new();

    internal readonly NodeConfig Config;

    public NodeWorld(NodeConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        Config = config;
    }

    [PublicAPI]
    public NodeSimulation CreateSimulation(
        int chunkWidth = ChunkConstants.DefaultWidth,
        int chunkHeight = ChunkConstants.DefaultHeight,
        int chunkDepth = ChunkConstants.DefaultDepth)
    {
        var simulation = new NodeSimulation(this, chunkWidth, chunkHeight, chunkDepth);
        return simulation;
    }
    
    [PublicAPI]
    public bool TryGetSimulation(SimulationId id, out NodeSimulation? simulation)
    {
        return _simulations.TryGetValue(id.GenId, out simulation);
    }
    
    [PublicAPI]
    public void Register<TNode, TNet>(NodeRule<TNode> rule, byte layerCapacity = 1)
        where TNode : INode
        where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>
    {
        foreach (ref var simulation in _simulations)
        {
            simulation.Register<TNode, TNet>(rule, layerCapacity);
        }
    }
    
    [PublicAPI]
    public void RegisterID<TNet>(NodeRule rule, int id)
        where TNet : INodeNet, INodeNetCreator<TNet>
    {
        foreach (ref var simulation in _simulations)
        {
            simulation.RegisterID<TNet>(rule, id);
        }
    }

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
        _simulations.Add(simulation, out var genId);
        return new SimulationId(genId);
    }
}
