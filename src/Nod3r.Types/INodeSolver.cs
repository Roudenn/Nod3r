using System.Diagnostics.CodeAnalysis;
using Numos.Maths;

namespace Nod3r.Types;

/// <summary>
/// Interface to interact with a node solver while inside a <see cref="INodeRule{T}"/> or from the node kernel.
/// </summary>
public interface INodeSolver
{
    /// <summary>
    /// Attempts to get a node at a specified voxel.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="pos"></param>
    /// <param name="layer"></param>
    /// <param name="node">The found node.</param>
    /// <typeparam name="T">Type of the node.</typeparam>
    /// <returns>True if the node was found.</returns>
    bool TryGetNode<T>(NodeChunkHandle chunk, Int3 pos, int layer, [NotNullWhen(true)] out T? node) where T : INode;
    
    /// <summary>
    /// Attempts to get a node at a specified voxel.
    /// </summary>
    /// <param name="voxel"></param>
    /// <param name="node">The found node.</param>
    /// <typeparam name="T">Type of the node.</typeparam>
    /// <returns>True if the node was found.</returns>
    bool TryGetNode<T>(NodeVoxelHandle voxel, [NotNullWhen(true)] out T? node) where T : INode;
    
    /// <summary>
    /// Gets a node voxel of a certain type that is relative
    /// to another voxel on the specified layer, and ensures that it exists.
    /// </summary>
    /// <param name="voxel">The origin node.</param>
    /// <param name="offset">Offset relative to the origin voxel to check for the target.</param>
    /// <param name="layer">Target layer to search the node in.</param>
    /// <param name="relative">The node that was found at that position.</param>
    /// <returns>True if the node was found on a relative position.</returns>
    bool TryGetRelative<T>(NodeVoxelHandle voxel, Int3 offset, int layer, out NodeVoxel relative) where T : INode;
    
    /// <summary>
    /// Gets a node voxel of a certain type that is relative
    /// to another voxel on the same layer as the origin, and ensures that it exists.
    /// </summary>
    /// <param name="voxel">The origin node.</param>
    /// <param name="offset">Offset relative to the origin voxel to check for the target.</param>
    /// <param name="relative">The node that was found at that position.</param>
    /// <returns>True if the node was found on a relative position.</returns>
    bool TryGetRelative<T>(NodeVoxelHandle voxel, Int3 offset, out NodeVoxel relative) where T : INode;
    
    bool TryGetRelative(int id, NodeVoxelHandle voxel, Int3 offset, out NodeVoxel relative);

    bool TryGetRelative(NodeVoxel voxel, Int3 offset, out NodeVoxel relative);
    
    bool TryGetRelative(NodeVoxel voxel, Int3 offset, int layer, out NodeVoxel relative);
    
    /// <summary>
    /// Marks the target voxel as dirty, which means it itself or one of its neighbors were changed.
    /// </summary>
    /// <param name="voxel">The target voxel to mark as dirty.</param>
    /// <returns>True if the voxel existed and marked as dirty, false if the voxel doesn't exist.</returns>
    bool DirtyVoxel(NodeVoxel voxel);
    
    bool DirtyVoxel<T>(NodeVoxelHandle voxel) where T : INode;
}
