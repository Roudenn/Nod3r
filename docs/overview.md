# Overview

Nod3r is an engine-agnostic, voxel-based node network connection solver. Nodes are "blocks" in space, and according to some custom rules they can connect together, forming a node network (a graph).

The library intents to replace slow and unexpandable `NodeGroupSystem` in the disaster simulation and roleplaying game Space Station 14.
However, Nod3r is still designed to be usable by other games that may need to connect a lot of objects in a voxel grid space.

## Core Design
Nod3r aims for good performance and multithreading, while allowing high level of customization using C# templates and more functionality for dynamic graph structures.

Space Station 14's `NodeGroupSystem` had many issues, which this project was designed to fix:
- Graph construction wasn't dynamic - this makes any rebuild of the node network extremely slow (around O(n^2) in runtime and O(n) on initialization).
- Every graph rebuild creates new node network objects and abandons the old ones, adding more work for the garbage collector.
- The system was heavily connected to the Entity-Component-System methods, which makes the usage of this system much more limited.

