# Overview

Nod3r is an engine-agnostic, voxel-based node network connection solver. Nodes are "blocks" in space, and according to some custom rules they can connect together, forming a node network.

The library intents to replace slow and unexpandable `NodeGroupSystem` in the disaster simulation and roleplaying game Space Station 14.
However, Nod3r is still designed to be usable by other games that may need to connect a lot of objects in a voxel grid space.

Nod3r aims for good performance and multithreading, while allowing high level of customization using C# templates.
