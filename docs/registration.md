# Node types & Registration

## Main types

To actually do anything with the simulation, you must provide the storage with 3 main types in order to properly add nodes and create networks out of them.

### Nodes (`INode` interface)

Node is a simple voxel unit stored in the chunk space of the solver. It's a block that holds the data necessary for the parent node network and connection rules with other nodes.

### Node networks (`INodeNet`)

Node networks are structures that aren't positioned anywhere specifically, they are just graphs that contain all connected nodes. Node networks can store the shared data about a network, and also they can iterate through their elements in a sequence starting from a specific node and branching off when encountering a fork.

Node networks also require to implement an `INodeNetCreator<TSelf>` interface, which just creates a new instance of the specified network. This is needed to prevent the usage of `Activator` class, since it causes a boxing allocation when creating struct instaces.

### Node rules (`INodeRule`)

Node rules only contain code (and maybe some buffers) that take in a node and finds all of its neighbors it can connect to. It can access the node kernel's API to get the relative nodes and their node data through the `INodeKernel` interface.

Node rules also require to implement an `INodeRuleCreator<TSelf>` interface to create a new rule instance for every active solver thread.

## Registering a node

To register a node in a `NodeSolver` instance, you must also choose an `INodeNet` type that will be created by this node type and the `INodeRule` type that will be used to find node connections.

After that, you can either register it in the registration delegate of `NodeConfig` that is passed to the `NodeSolver`, or register it in runtime using an API method (this may lead to lag spikes, so it's not recommended to do).
