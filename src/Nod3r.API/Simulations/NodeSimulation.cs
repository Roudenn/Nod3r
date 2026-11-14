using JetBrains.Annotations;
using Nod3r.API.Registration;
using Nod3r.Solver;
using Nod3r.Solver.Chunks;
using Nod3r.Solver.Kernels;
using Numos.Maths;

namespace Nod3r.API.Simulations;

/// <summary>
/// A solver instance that allows manipulating a chunk map of nodes
/// and rebuild node networks inside of it.
/// </summary>
/// <para>
/// A solver is a continuous grid where grids can interact with each other directly.
/// Multiple simulation instances can be connected together in context of a <see cref="NodeWorld"/>
/// with explicit links.
/// </para>
/// <para>
/// Each solver wraps a node kernel instance for every registered node type <see cref="NodeIdx"/>.
/// Such division right at the top makes the internals much nicer and faster.
/// </para>
public sealed partial class NodeSimulation
{
    /// <summary>
    /// Registered amount of node types in this solver instance.
    /// </summary>
    public int RegistrationCount { get; private set; }
    
    /// <summary>
    /// An array that converts <see cref="NodeIdx"/> value into a local index
    /// that can be used to get a proper <see cref="INodeKernel"/>.
    /// </summary>
    private int[] _idxes = [];
    
    /// <summary>
    /// A <see cref="NodeWorld"/> that owns this simulation.
    /// </summary>
    [PublicAPI]
    public NodeWorld World { get; }
    
    /// <summary>
    /// ID of this simulation in the registered <see cref="NodeWorld"/> storage.
    /// </summary>
    [PublicAPI]
    public SimulationId Id { get; }
    
    /// <summary>
    /// Dimensions for all chunks in the simulation.
    /// </summary>
    [PublicAPI]
    public Int3 Dimensions { get; }
    
    /// <summary>
    /// All kernels in a sequence controlled by this simulation.
    /// </summary>
    private INodeKernel[] _kernels = [];

    /// <summary>
    /// All chunks that were registered at least in a single kernel.
    /// </summary>
    private readonly SortedSet<NodeChunkHandle> _registeredChunks = new();
    
    /// <summary>
    /// Creates a new node simulation instance, registering it in a world.
    /// </summary>
    /// <param name="world">World to register this simulation in.</param>
    /// <param name="chunkDepth">Depth of every chunk.</param>
    /// <param name="chunkHeight">Height of every chunk.</param>
    /// <param name="chunkWidth">Width of every chunk.</param>
    /// <exception cref="ArgumentNullException">
    /// The provided world was null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Chunk dimensions are zero or negative.
    /// </exception>
    public NodeSimulation(
        NodeWorld world,
        int chunkDepth = ChunkConstants.DefaultDepth,
        int chunkHeight = ChunkConstants.DefaultHeight,
        int chunkWidth = ChunkConstants.DefaultWidth)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkHeight);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkDepth);
        
        World = world;
        Dimensions = new Int3(chunkWidth, chunkHeight, chunkDepth);
        Id = world.RegisterSimulation(this);
    }

    private INodeKernel GetKernel(VoxelRef voxel) => _kernels[_idxes[voxel.Type.Value]];
    
    private INodeKernel<T> GetKernel<T>() where T : INode
        => (INodeKernel<T>) _kernels[_idxes[NodeIdxStorage.Get<T>().Value]];

    private INodeNetKernel<T> GetKernelNet<T>()
        where T : INodeNet
        => (INodeNetKernel<T>) _kernels[_idxes[NodeIdxStorage.GetNet<T>().Value]];
    
    private INodeIDKernel GetIDKernel(int id)
        => (INodeIDKernel) _kernels[_idxes[NodeIdxStorage.Get(id).Value]];
    
    private INodeNetKernel<T> GetIDKernelNet<T>(int id)
        where T : INodeNet
        => (INodeNetKernel<T>) _kernels[_idxes[NodeIdxStorage.Get(id).Value]];
}
