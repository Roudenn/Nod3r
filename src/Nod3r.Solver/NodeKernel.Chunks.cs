using Numos.Maths;

namespace Nod3r.Solver;

// Contains API methods to interact with chunks.
internal sealed partial class NodeKernel<TNode, TNet>
{
    /// <summary>
    ///     Returns a detached list of the currently registered chunk-grid positions.
    /// </summary>
    public Int3[] GetChunkPositions() => _chunkMap.Keys.ToArray();
    
    public bool TryGetChunk(Int3 position, out NodeChunk chunk)
    {
        return _chunkMap.TryGetValue(position, out chunk!);
    }
    
    public bool HasChunk(Int3 position) => _chunkMap.ContainsKey(position);
    
    /// <summary>
    ///     Returns registered positions only when the chunk collection changed.
    /// </summary>
    public bool TryGetChunkPositions(
        long knownRevision,
        out long revision,
        out Int3[] positions)
    {
        revision = CollectionRevision;
        if (revision == knownRevision)
        {
            positions = [];
            return false;
        }

        positions = _chunkMap.Keys.ToArray();
        return true;
    }
    
    
    
    /// <summary>
    ///     Adds the specified chunk to the chunk map internal dictionary and reserves a <see cref="Chunk.DenseId"/>.
    /// </summary>
    /// <param name="chunk">The chunk object to register.</param>
    /// <exception cref="ArgumentException">Chunk dimensions didn't match the dimensions of the chunk map.</exception>
    /// <exception cref="InvalidOperationException">A chunk is already registered at the specified position.</exception>
    public void RegisterChunk(NodeChunk chunk)
    {
        if (chunk.Dimensions != Dimensions)
            throw new ArgumentException("Chunk dimensions must match the simulation.", nameof(chunk));

        if (!_chunkMap.TryAdd(chunk.GridPosition, chunk))
            throw new InvalidOperationException($"A chunk is already registered at {chunk.GridPosition}.");

        chunk.DenseId = FreeDenseIds.Count > 0 ? FreeDenseIds.Pop() : NextDenseId++;
        CollectionRevision++;
    }

    /// <summary>
    ///     Removes and releases the chunk at a grid position.
    /// </summary>
    /// <param name="position">The chunk-grid position to remove.</param>
    /// <returns><see langword="true" /> if a chunk was removed; otherwise, <see langword="false" />.</returns>
    public bool UnregisterChunk(Int3 position)
    {
        if (!_chunkMap.TryRemove(position, out var chunk))
            return false;
        
        FreeDenseIds.Push(chunk.DenseId);
        CollectionRevision++;
        return true;
    }

    /// <summary>
    ///     Creates, initializes, and registers a chunk owned by this kernel.
    /// </summary>
    /// <param name="position">The chunk's position in the chunk grid.</param>
    /// <exception cref="InvalidOperationException">A chunk is already registered at <paramref name="position" />.</exception>
    public void CreateChunk(Int3 position)
    {
        if (_chunkMap.ContainsKey(position))
            throw new InvalidOperationException($"A chunk is already registered at {position}.");

        new NodeChunk(position, Dimensions);
    }
    
    /// <summary>
    ///     Gets a chunk from the chunk map at a specific position.
    /// </summary>
    /// <param name="position">Position to get the chunk at.</param>
    /// <returns>Chunk object stored at that position.</returns>
    /// <exception cref="KeyNotFoundException">Specified position doesn't have a chunk.</exception>
    public NodeChunk GetChunk(Int3 position)
    {
        if (_chunkMap.TryGetValue(position, out var chunk))
            return chunk;

        throw new KeyNotFoundException($"No {nameof(NodeChunk)} is registered at ({position.X}, {position.Y}, {position.Z}).");
    }
    
    /// <summary>
    ///     Returns an array of all chunks ordered by their position.
    /// </summary>
    public NodeChunk[] OrderedChunks()
    {
        return _chunkMap.Values
            .OrderBy(static chunk => chunk.GridPosition.X)
            .ThenBy(static chunk => chunk.GridPosition.Y)
            .ThenBy(static chunk => chunk.GridPosition.Z).ToArray();
    }
    
    /*/// <summary>
    ///     Gets a voxel index in the array while validating its position.
    /// </summary>
    /// <param name="chunk">A chunk to get the voxel index from.</param>
    /// <param name="x">The X coordinate of the voxel.</param>
    /// <param name="y">The Y coordinate of the voxel.</param>
    /// <param name="z">The Z coordinate of the voxel.</param>
    /// <returns>A voxel index that is guaranteed to be valid for this chunk.</returns>
    /// <exception cref="ArgumentOutOfRangeException">X, Y, or Z coordinate isn't in the dimensions of the chunk.</exception>
    public static ushort GetValidatedVoxelIndex(NodeChunk chunk, int x, int y, int z)
    {
        if (x < 0 || x >= chunk.Width)
            throw new ArgumentOutOfRangeException(nameof(x));

        if (y < 0 || y >= chunk.Height)
            throw new ArgumentOutOfRangeException(nameof(y));

        if (z < 0 || z >= chunk.Depth)
            throw new ArgumentOutOfRangeException(nameof(z));

        return chunk.GetIndex(x, y, z);
    }

    /// <summary>
    ///     Validates that the given local voxel index is within the bounds of the chunk's voxel array.
    /// </summary>
    /// <param name="chunk">The chunk to validate against.</param>
    /// <param name="localVoxelIndex">The local voxel index to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the local voxel index is out of bounds.</exception>
    public static void ValidateVoxelIndex(NodeChunk chunk, ushort localVoxelIndex)
    {
        if (localVoxelIndex >= chunk.VoxelCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(localVoxelIndex),
                localVoxelIndex,
                $"Voxel index must be less than the chunk's voxel count ({chunk.VoxelCount}).");
        }
    }*/
    
    public void Dispose()
    {
        
    }

    public void GetChunkData(Int3 handle, List<(TNode Data, Int3 Pos)> set, out Int3 dimensions)
    {
        var chunk = GetChunk(handle);
        var cache = new List<TNode>(NodeStorage.LayerCapacity);
        dimensions = chunk.Dimensions;
        for (int z = 0; z < chunk.Dimensions.Z; z++)
        {
            for (int y = 0; y < chunk.Dimensions.Y; y++)
            {
                for (int x = 0; x < chunk.Dimensions.X; x++)
                {
                    var pos = new Int3(x, y, z);
                    var column = chunk.Handles[pos];
                    if (!column.IsValid)
                        continue;
                    
                    NodeStorage.GetColumnData(column, cache);
                    foreach (var data in cache)
                    {
                        set.Add((data, pos));
                    }
                }
            }
        }
    }
}
