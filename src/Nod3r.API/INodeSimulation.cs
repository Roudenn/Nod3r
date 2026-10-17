using System.Diagnostics.CodeAnalysis;
using Nod3r.Solver;
using Numos.Maths;

namespace Nod3r.API;

/// <summary>
/// Interface to interact with a node solver while inside a <see cref="NodeRule{T}"/> or from the node kernel.
/// </summary>
public interface INodeSimulation
{
    /// <summary>
    /// Attempts to get a node at a specified voxel.
    /// </summary>
    /// <param name="voxel"></param>
    /// <param name="node">The found node.</param>
    /// <typeparam name="T">Type of the node.</typeparam>
    /// <returns>True if the node was found.</returns>
    bool TryGetNode<T>(VoxelRef voxel, [NotNullWhen(true)] out T? node) where T : INode;
    
    /// <summary>
    /// Gets a node voxel of a certain type that is relative
    /// to another voxel on the specified layer, and ensures that it exists.
    /// </summary>
    /// <param name="voxel">The origin node.</param>
    /// <param name="offset">Offset relative to the origin voxel to check for the target.</param>
    /// <param name="layer">Target layer to search the node in.</param>
    /// <param name="relative">The node that was found at that position.</param>
    /// <returns>True if the node was found on a relative position.</returns>
    bool TryGetRelative<T>(VoxelRef voxel, Int3 offset, byte layer, out VoxelRef relative) where T : INode;
    
    /// <summary>
    /// Gets a node voxel of a certain type that is relative
    /// to another voxel on the same layer as the origin, and ensures that it exists.
    /// </summary>
    /// <param name="voxel">The origin node.</param>
    /// <param name="offset">Offset relative to the origin voxel to check for the target.</param>
    /// <param name="relative">The node that was found at that position.</param>
    /// <returns>True if the node was found on a relative position.</returns>
    bool TryGetRelative<T>(VoxelRef voxel, Int3 offset, out VoxelRef relative) where T : INode;
    
    bool TryGetRelative(int id, VoxelRef voxel, Int3 offset, out VoxelRef relative);

    bool TryGetRelative(VoxelRef voxel, Int3 offset, out VoxelRef relative);
    
    bool TryGetRelative(VoxelRef voxel, Int3 offset, byte layer, out VoxelRef relative);
    
    /// <summary>
    /// Marks the target voxel as dirty, which means it itself or one of its neighbors were changed.
    /// </summary>
    /// <param name="voxel">The target voxel to mark as dirty.</param>
    /// <returns>True if the voxel existed and marked as dirty, false if the voxel doesn't exist.</returns>
    bool DirtyVoxel(VoxelRef voxel);
    
    bool DirtyVoxel<T>(VoxelRef voxel) where T : INode;
}
