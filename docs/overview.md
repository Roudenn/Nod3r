# Overview

Nod3r is an engine-agnostic, voxel-based node network connection solver. Nodes are "blocks" in space, and according to some custom rules they can connect together, forming a node network (a graph).

Nod3r can be useful for voxel or tile based games for connecting large amounts of objects placed in a grid,
as well as physics simulations that use the finite difference method.

## Highlights

Nod3r aims for good performance and multithreading, while also allowing a high level of customization.

**Highlights for some main features:**
- Support for both simple boolean nodes and complex C# type nodes
- High memory locality
- Parallel and background-thread support
- Support for non-standard space topology (portals)

## Project structure

Nod3r is split into 4 projects (dependent on each other in order):
- `Nod3r.Collections` - General collections used by Nod3r internally. Mostly includes the generation storages of multiple kinds, which are used for storing nodes in large monolith arrays and providing direct access to them with `ref return`s.
- `Nod3r.Solver` - The core part of the project, which includes the basic types, node kernels and implementations for node storage and chunks. Doesn't do anything on its own and requires `Nod3r.API` project to interact with.
- `Nod3r.API` - API methods to interact with `Nod3r.Solver`, also includes the most useful `NodeSimulation` and `NodeWorld` spaces.
- `Nod3r.Examples` - General and example implementations of nodes and node networks.
