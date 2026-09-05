# Nod3r Storage types and methods

## Data structure overview

Nod3r mostly uses 3 data structures: `FlatArray` from Numos to store chunk data (references for nodes), `GenIdStorage` to store existing node networks, and `Gen2DStorage` to store nodes themselves. They come from the `Numos.Collections` package and the `Nod3r.Collections` project.

Obviously, the amount of nodes in voxel chunks is almost always lower than the maximum amount of nodes a chunk can hold. That's why Nod3r uses an evolving data structure instead of pre-allocating all memory for every node type (since that would be a complete waste of memory).

## `GenIdStorage`

Nodes are stored in a generational ID storage, specifically it's 2D version `Gen2DStorage`. Generational ID storage is basically an evolving array that returns a `GenId` struct when an element is added. `GenId` is basically a key to an object stored in the `GenIdStorage`, which specifies at which index and which generation the target element is living.

Removal of elements in GenIdStorage though is almost instant, since instead of actually removing the object, instead the generation of the slot is increased and it's marked as free. That way the next addition can safely overwrite an already existing object instead of allocating new space.

`GenIdStorage` is great for managing lots of structs, since it allows to use the memory efficiently. However, internally it is still just an array. When a storage runs out of space, it has to reallocate itself to a new spot in memory, which might be quite expensive in case if there are a lot of living nodes. That's why it's **highly recommended** to specify the default capacity of the Node Storage to a big enough number that is exceeded only in very special cases.

## `Gen2DStorage`

Now, back to the 2D part of the `Gen2DStorage`. Basically this storage is a `GenIdStorage`, but nested twice, meaning it's an evolving array of evolving arrays. This is needed for the support of node layers, so multiple nodes of the same type can be added on the same voxel. Instead of `GenId` the 2D version of the storage uses `LayerId`, which is a `ColumnHandle`, and `int` layer, and a Generation of the added element wrapped together. The actual difference from `GenIdStorage` is that internally node chunks store only the `ColumnHandle`s, which reference a column of nodes stored in that voxel.

`Gen2DStorage` is able to reference an entire evolving array of nodes stored on a specific `ColumnHandle`, and combined with an `int` layer it can get a specific element from the storage internally. Each column can have a different maximum size, which allows to handle edge cases when tons of nodes were stacked together without expanding the entire storage for that.

The main problem with `Gen2DStorage` is that it may become sparse in case if too many column reallocations will happen too late into the program's runtime. That's why if you want to avoid performance degradation caused by sparse memory positioning, **you have to limit the amount of same nodes on the same voxel** manually and pre-allocate that maximum space when registering the node type.
