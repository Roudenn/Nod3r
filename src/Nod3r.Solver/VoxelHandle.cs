using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Represents a position of a specific node in the node kernel.
/// </summary>
/// <param name="ChunkPos">The target chunk.</param>
/// <param name="VoxelIndex">The flat local voxel index within the chunk.</param>
/// <param name="Layer">Layer of the voxel.</param>
internal record struct VoxelHandle(Int3 ChunkPos, ushort VoxelIndex, byte Layer);
