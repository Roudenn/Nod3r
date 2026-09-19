# Nodes

Nod3r supports 2 categories of nodes - typed nodes as C# structs that inherit from `INode` interface, and ID nodes which don't contain any data and are just flags.

## ID nodes

ID nodes are nodes that don't contain any specific data. They can only be placed and removed, and node rules for them are static (in a sense that they always return the same relative results for every node).

ID nodes are stored directly as bytes in a chunk, where each byte represents a segment of 8 nodes. With that technique these nodes are extremely memory efficient, so even a large amount of nodes comparatively to Type nodes won't create any serious performance issues.

## Type nodes (`: INode`)

Type nodes are structs that contain specific data about a voxel in a node network they're in.
This allows to do complex operations on the node network's graph, such as simulating atmospherics inside pipes, or simulating electricity and current resistance of a power grid, etc.

Type nodes are not stored in chunks directly, instead they're referenced by the

While allowing for much more flexibility, Type nodes have worse memory efficiency compared to ID nodes. If not accounting for metadata, Type nodes are referenced by a 32-bit index and have a minimal size of 8 bits.
ID nodes on the other hand take up only 1 bit in memory, which is 40 times better.

## Type nodes VS ID nodes

Use ID nodes when each node doesn't have special properties compared to other nodes. Sometimes you may even be able to get away with very rare differences in nodes by storing these changes in the node network of the nodes.

Use Type nodes when you are making a complex system where each node may contain unique data. This includes complex simulations where each node can have a completely unique state.
