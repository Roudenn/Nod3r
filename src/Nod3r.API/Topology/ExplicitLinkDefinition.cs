namespace Nod3r.API.Topology;

/// <summary>
///     Defines one undirected sparse voxel adjacency.
/// </summary>
/// <param name="First">One endpoint. Registration canonicalizes endpoint orientation.</param>
/// <param name="Second">The other endpoint.</param>
public readonly record struct ExplicitLinkDefinition(
    VoxelRef First,
    VoxelRef Second);
