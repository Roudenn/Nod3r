# Nodes

Nod3r supports 2 categories of nodes:
- Typed nodes as C# structs that inherit from `INode` interface;
- ID nodes which don't contain any data and either exist or don't at a certain spot.

The guide below should help to choose which one is better to use in which scenarios.

## ID nodes

ID nodes are nodes that don't contain any specific data. They can only be placed and removed, and node rules for them are static (in a sense that they always return the same relative results for every node).

ID nodes are stored directly as bytes in a chunk, where each byte represents a segment of 8 nodes. With that technique these nodes are extremely memory efficient, so even a large amount of nodes comparatively to Type nodes won't create any serious performance issues.

## Type nodes (`: INode`)

Type nodes are structs that contain specific data about a voxel in a node network they're in.
This allows to do complex operations on the node network's graph, such as simulating atmospherics inside pipes, or simulating electricity and current resistance of a power grid, etc.

While allowing for much more flexibility, Type nodes have worse memory efficiency compared to ID nodes, because of the way node storage is implemented (see (storage guide)[storage.md]).

## Type nodes VS ID nodes

Use ID nodes when each node doesn't have special properties compared to other nodes, or the size of the data per every node is smaller than ~32 bits per every node.
When node states are relatively similar to each other, it's likely more efficient to just store them in the node network instead of converting into Type nodes.

Use Type nodes when you are making a complex system where each node may contain unique data. This includes complex simulations where each node can have a completely unique state.
