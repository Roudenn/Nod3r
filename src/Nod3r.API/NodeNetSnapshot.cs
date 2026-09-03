using Nod3r.Collections;
using Nod3r.Types;

namespace Nod3r.API;

/// <summary>
/// A read-only snapshot of a node network. 
/// </summary>
/// <param name="Net">The custom data about this network.</param>
/// <param name="GenId"><see cref="GenId"/> of this network in the shared node network storage.</param>
/// <param name="Nodes">A list of <see cref="LayerId"/> references to all nodes controlled by this network.</param>
/// <typeparam name="T">Type of network stored in this snapshot.</typeparam>
public readonly record struct NodeNetSnapshot<T>(T Net, GenId GenId, IReadOnlySet<LayerId> Nodes) where T : INodeNet;
