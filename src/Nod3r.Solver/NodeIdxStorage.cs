using Nod3r.Types;

namespace Nod3r.Solver;

/// <summary>
/// Shared storage for all <see cref="NodeIdx"/>.
/// </summary>
// TODO consider whether this should be exposed or not
public static class NodeIdxStorage
{
    /// <summary>
    /// Amount of nodes registered in the program.
    /// </summary>
    public static int Count { get; private set; }

    /// <summary>
    /// Registers a type in the program.
    /// </summary>
    /// <typeparam name="T">Type of node.</typeparam>
    internal static void Register<T>() where T : INode
    {
        Storage<T>.Index = new NodeIdx(Count);
        Count++;
    }

    /// <summary>
    /// Gets the <see cref="NodeIdx"/> of a node type.
    /// </summary>
    /// <typeparam name="T">Type of node.</typeparam>
    /// <returns><see cref="NodeIdx"/> representing this type.</returns>
    /// <exception cref="InvalidOperationException">The specified type wasn't registered in the program.</exception>
    public static NodeIdx Get<T>() where T : INode
    {
        var idx = Storage<T>.Index;
        return idx == NodeIdx.Invalid
            ? throw new InvalidOperationException($"Tried to get a {nameof(NodeIdx)} for a node type that wasn't registered!")
            : idx;
    }
    
    /// <summary>
    /// A helper static class automatically creates a separate static instance for each registered node type.
    /// </summary>
    /// <typeparam name="T">The controlled node type.</typeparam>
    private static class Storage<T> where T : INode
    {
        // Analyzer suppression is intentional, because here we actually want this field
        // to be different for different generated node types.
        // ReSharper disable once StaticMemberInGenericType
        public static NodeIdx Index = NodeIdx.Invalid;
    }
}
