using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nod3r.Collections;

// GenIdStorage implementation taken and heavily modified from Space Station 14 under the MIT license

/// <summary>
/// An implementation of a generational ID storage.
/// </summary>
/// <remarks>
/// <para>
/// This is a very basic implementation that has the ability to automatically expand the internal array,
/// return direct references to its elements by indexing and during enumeration,
/// and also has optimizations for sparse iteration.
/// </para>
/// <para>
/// This collection is most useful for enumerating, modifying and removing large collections
/// of an undefined amount of unordered structs that get added and removed at any time.
/// </para>
/// <para>
/// When freeing a slot, instead of removing the data it is only marked as deleted,
/// allowing the next allocation to overwrite an already existing spot in memory.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of data to store.</typeparam>
public class GenIdStorage<T>
{
    /// <summary>
    /// Index of the next free slot.
    /// Equals to <see cref="int.MaxValue"/> when the storage is full.
    /// </summary>
    protected int NextFree;

    /// <summary>
    /// Data stored in every slot.
    /// </summary>
    protected internal T[] Data;
    
    /// <summary>
    /// Next link on the free list for each slot. if int.MaxValue then this is the tail.
    /// If negative, this slot is occupied.
    /// </summary>
    protected int[] NextSlots;
    
    /// <summary>
    /// Current generation for each slot.
    /// Allows for instant deletion by incrementing the generation by 1.
    /// </summary>
    protected int[] Generations;
    
    /// <summary>
    /// Dense index of every slot.
    /// </summary>
    protected int[] DenseIndex;
    
    /// <summary>
    /// Pointer to a position in a dense index array.
    /// Allows for O(Count) iteration instead of O(Length).
    /// </summary>
    protected internal int[] Dense;

    /// <summary>
    /// Total amount of stored objects.
    /// </summary>
    public int Count { get; private set; }
    
    /// <summary>
    /// Current maximum length of the internal arrays in the storage.
    /// </summary>
    public int Length { get; private set; }

    public GenIdStorage(int capacity = 16)
    {
        Length = capacity;
        
        Data = new T[capacity];
        NextSlots = new int[capacity];
        Generations = new int[capacity];
        DenseIndex = new int[capacity];
        Dense = new int[capacity];
        
        for (int i = 0; i < capacity; i++)
        {
            // Build linked list chain for newly allocated segment.
            NextSlots[i] = i == capacity - 1 ? NextFree : i + 1;
            // Every slot starts at generation 1.
            Generations[i] = 1;
        }
    }
    
    public T this[GenId id]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)id.Index >= (uint)Data.Length)
                ThrowKeyNotFound();
   
            if (Generations[id.Index] != id.Generation)
                ThrowKeyNotFound();

            return Data[id.Index];
        }
    }

    public bool IsValid(GenId id)
    {
        if ((uint)id.Index >= (uint)Data.Length)
            return false;
   
        return Generations[id.Index] == id.Generation;
    }
    
    public virtual void Add(T value, out GenId id)
    {
        if ((uint)NextFree >= (uint)Data.Length)
            ReAllocate();

        var idx = NextFree;
        
        Dense[Count] = idx;
        DenseIndex[idx] = Count;
        Count += 1;
        NextFree = NextSlots[idx];
        NextSlots[idx] = -1; // Means filled

        id = new GenId(idx, Generations[idx]);
        Data[idx] = value;
    }

    public GenId Add(T value)
    {
        Add(value, out var id);
        return id;
    }

    public virtual void Free(GenId id)
    {
        if ((uint)id.Index >= (uint)Data.Length)
            ThrowKeyNotFound();
        
        if (Generations[id.Index] != id.Generation || NextSlots[id.Index] >= 0)
            ThrowKeyNotFound();

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Data[id.Index] = default!;

        int removedDenseIdx = DenseIndex[id.Index];
        int lastDenseIdx = Count - 1;
    
        // Swap-and-pop inside dense array only if it's not the last element
        if (removedDenseIdx != lastDenseIdx)
        {
            int lastSlotIdx = Dense[lastDenseIdx];
            Dense[removedDenseIdx] = lastSlotIdx;
            DenseIndex[lastSlotIdx] = removedDenseIdx;
        }
        
        Count -= 1;
        Generations[id.Index] += 1;
        NextSlots[id.Index] = NextFree;
        NextFree = id.Index;
    }

    /// <summary>
    /// Ensures that the capacity of this storage is at least the specified <paramref name="capacity"/>.
    /// If the current capacity is less than <paramref name="capacity"/>,
    /// it is increased to at least the specified <paramref name="capacity"/>.</summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this storage.</returns>
    public int EnsureCapacity(int capacity)
    {
        if ((uint)capacity < (uint)Length)
            return Length;
        
        ReAllocateTo(Math.Max(Math.Max(Length, 2) * 2, capacity));
        return capacity;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ReAllocate()
    {
        int oldLength = Data.Length;
        int newLength = Math.Max(oldLength, 2) * 2;

        ReAllocateTo(newLength);
    }

    protected virtual void ReAllocateTo(int newSize)
    {
        int oldLength = Length;
        Debug.Assert(newSize >= oldLength, "Cannot shrink GenIdStorage");
        
        Length = newSize;
        
        Array.Resize(ref Data, newSize);
        Array.Resize(ref NextSlots, newSize);
        Array.Resize(ref Generations, newSize);
        Array.Resize(ref DenseIndex, newSize);
        Array.Resize(ref Dense, newSize);

        for (int i = oldLength; i < newSize; i++)
        {
            // Build linked list chain for newly allocated segment.
            NextSlots[i] = i == newSize - 1 ? NextFree : i + 1;
            // Every slot starts at generation 1.
            Generations[i] = 1;
        }

        NextFree = oldLength;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowKeyNotFound()
    {
        throw new KeyNotFoundException();
    }
}

public readonly record struct GenId(int Index, int Generation)
{
    public readonly static GenId Invalid = new(0, 0);
    
    public bool IsValid() => Generation > 0;
    
    public override string ToString()
    {
        return $"{Index} (G{Generation})";
    }
}
