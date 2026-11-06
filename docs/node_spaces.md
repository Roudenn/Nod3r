# Node spaces

"Node spaces" is a general name for **node worlds**, **node simulations** and **node kernels**.

## The flowchart

Scheme below shows how node spaces depend on each other.

```mermaid
flowchart TD
    NodeWorld
    style NodeWorld stroke:#6b9bb3'
    NodeSolver1
    style NodeSolver1 stroke:#6b9bb3'
    NodeSolver2
    style NodeSolver2 stroke:#6b9bb3'
    NodeKernel1
    style NodeKernel1 stroke:#6b9bb3'
    NodeKernel2
    style NodeKernel2 stroke:#6b9bb3'
```

## Node Kernel

Node kernels are the smallest units for complete node spaces. They control a specific node type or ID and store the chunk data.

Node kernels are completely internal, they're created and controlled automatically when a new node ID/Type is registered in a `NodeSimulation`.

## Node Simulation

A node simulation represents a continuous grid of nodes of different types.

Each node simulation may have its own chunk size and also registered types

This is the class that contains the most important API to interact with chunks and nodes.

## Node World

Node world is a collection of one or multiple node simulations which share the same configuration.

Simulations under the same `NodeWorld` can connect together with explicit topology (or simply speaking, with portals).
