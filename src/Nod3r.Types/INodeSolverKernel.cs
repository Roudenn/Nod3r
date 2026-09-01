using System.Diagnostics.CodeAnalysis;
using Numos.Maths;

namespace Nod3r.Types;

/// <summary>
/// Interface that contains public API to interact with the node solver kernel.
/// Used for <see cref="INodeRule"/>s to access the kernel publicly.
/// </summary>
public interface INodeSolverKernel
{
    /// <summary>
    /// Attempts to get a node at a specified voxel.
    /// </summary>
    /// <param name="voxel">The target voxel to check.</param>
    /// <param name="node">The found node.</param>
    /// <typeparam name="T">Type of the node.</typeparam>
    /// <returns>True if the node was found.</returns>
    bool TryGetNode<T>(NodeVoxel voxel, [NotNullWhen(true)] out T? node) where T : INode;

    /// <summary>
    /// Attempts to get a node at a specified voxel.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="pos"></param>
    /// <param name="type"></param>
    /// <param name="layer"></param>
    /// <param name="node">The found node.</param>
    /// <typeparam name="T">Type of the node.</typeparam>
    /// <returns>True if the node was found.</returns>
    bool TryGetNode<T>(Int3 chunk, Int3 pos, NodeIdx type, int layer, [NotNullWhen(true)] out T? node) where T : INode;
    
    /// <summary>
    /// Gets a node voxel of a certain type that is relative
    /// to another voxel on the specified layer, and ensures that it exists.
    /// </summary>
    /// <param name="voxel">The origin node.</param>
    /// <param name="offset">Offset relative to the origin voxel to check for the target.</param>
    /// <param name="type">Type of the node to look for.</param>
    /// <param name="layer">Target layer to search the node in.</param>
    /// <param name="relative">The node that was found at that position.</param>
    /// <returns>True if the node was found on a relative position.</returns>
    bool TryGetRelative(NodeVoxel voxel, Int3 offset, NodeIdx type, int layer, out NodeVoxel relative);
    
    /// <summary>
    /// Gets a node voxel of a certain type that is relative
    /// to another voxel on the same layer as the origin, and ensures that it exists.
    /// </summary>
    /// <param name="voxel">The origin node.</param>
    /// <param name="offset">Offset relative to the origin voxel to check for the target.</param>
    /// <param name="type">Type of the node to look for.</param>
    /// <param name="relative">The node that was found at that position.</param>
    /// <returns>True if the node was found on a relative position.</returns>
    bool TryGetRelative(NodeVoxel voxel, Int3 offset, NodeIdx type, out NodeVoxel relative);
}
