# Node types & Registration

## Main types

To make any operation with the provided solver, a pair of two types must be registered first - a Node and a Node network.

### Nodes (`INode` interface)

A node is a simple voxel unit stored in the chunk space of the solver. It's a block that holds some unique data for a specific position.

### Node networks (`INodeNet`)

Node networks are structures that exist virtually in the kernel. They are formed when a node world/simulation is rebuilt out of an interconnected group of nodes, and then store shared data about that group.

## Registering a node

You can register a node/network pair in a `NodeWorld` either by passing an `Action<INodeRegistration>` in the constructor, or by calling the `Register()` method manually.

After the node/net pair was registered, it can be used freely from `NodeSimulation`'s API.
