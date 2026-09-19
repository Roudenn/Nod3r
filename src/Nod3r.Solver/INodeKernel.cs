using System.Diagnostics.CodeAnalysis;
using Nod3r.Collections;
using Nod3r.Types;
using Numos.Maths;

namespace Nod3r.Solver;

/// <summary>
/// Interface that contains public API to read information from the node kernel.
/// </summary>
internal interface INodeKernel
{
    bool HasChunk(Int3 position);

    void CreateChunk(Int3 position, int width, int height, int depth);
    
    bool RemoveNode(NodeVoxelHandle voxel);
    
    bool HasNode(NodeVoxelHandle voxel);
    
    bool TryGetRelative(NodeVoxelHandle node, Int3 offset, int layer, out NodeVoxelHandle relative);
    
    bool TryGetRelative(NodeVoxelHandle node, Int3 offset, out NodeVoxelHandle relative);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
internal interface INodeKernel<T> : INodeKernel
    where T : INode
{
    internal List<GenId> Nets { get; set; }
    
    internal NodeStorage<T> NodeStorage { get; set; }
    
    void SetNode(T node, NodeVoxelHandle voxel);
    
    void AddNode(T node, NodeChunkHandle chunk, Int3 pos);

    bool TryGetNode(NodeChunkHandle chunk, Int3 pos, int layer, [NotNullWhen(true)] out T? node);
}

internal interface INodeKernel<TNode, TNet> : INodeKernel<TNode>
    where TNode : INode
    where TNet : INodeNet
{
    internal NodeNetStorage<TNet> NodeNetStorage { get; set; }
}
