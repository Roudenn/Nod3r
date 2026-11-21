using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nod3r.Collections;

// GenIdStorage implementation taken and heavily modified from Space Station 14 under the MIT license

/// <summary>
/// A two-dimensional generational storage backed by <see cref="FixedSpaceArray{T}"/>.
/// Each column is one allocation in the contiguous backing array. Layer metadata
/// (generations and the free-layer list) remains owned by this wrapper.
/// </summary>
/// <remarks>
/// This type is not thread-safe. As with <see cref="FixedSpaceArray{T}"/>, do not
/// retain spans obtained from the backing allocator across allocation or free operations.
/// </remarks>
public sealed class Gen2DStorage<T>
{
    private readonly FixedSpaceArray<T> _storage;

    /// <summary>Head of the free-column list; int.MaxValue means there are no free columns.</summary>
    private int _nextFree;

    /// <summary>Next link in the free-column list. A negative value means occupied.</summary>
    private int[] _nextSlots;

    /// <summary>The allocation in FixedSpaceArray that currently stores each column.</summary>
    private GenId[] _columnAllocations;

    /// <summary>Head of the free-layer list for each column.</summary>
    private int[] _nextFreeLayers;

    /// <summary>Next link in each column's free-layer list. A negative value means occupied.</summary>
    private int[][] _nextSlotLayers;

    /// <summary>Current generation of each layer.</summary>
    internal int[][] Generations;

    /// <summary>Capacity of each column. Inactive columns retain their last capacity.</summary>
    private int[] _layerCapacities;

    /// <summary>Generation of each column, used to invalidate all its old LayerIds at once.</summary>
    internal int[] ColumnGenerations;

    /// <summary>Number of allocated layers in each column.</summary>
    private int[] _columnCounts;

    /// <summary>Total number of allocated layers.</summary>
    public int Count { get; private set; }

    /// <summary>Total number of allocated columns.</summary>
    public int ColumnCount { get; private set; }

    /// <summary>Current capacity of the column metadata arrays.</summary>
    public int Length { get; private set; }

    /// <summary>Default capacity assigned to newly created columns.</summary>
    public int LayerCapacity { get; private set; }

    /// <summary>Current length of the contiguous backing array used for column data.</summary>
    public int DataCapacity => _storage.Capacity;

    /// <summary>Amount of the backing array that has been used at least once (including freed blocks).</summary>
    public int UsedDataLength => _storage.UsedLength;

    /// <summary>
    /// Creates a new storage.
    /// </summary>
    /// <param name="capacity">Initial number of columns.</param>
    /// <param name="layerCapacity">Initial/default number of layers in a column.</param>
    public Gen2DStorage(int capacity = 16, byte layerCapacity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        Length = capacity;
        LayerCapacity = layerCapacity;
        Count = 0;
        ColumnCount = 0;
        _nextFree = capacity == 0 ? int.MaxValue : 0;

        _nextSlots = new int[capacity];
        _columnAllocations = new GenId[capacity];
        _nextFreeLayers = new int[capacity];
        _nextSlotLayers = new int[capacity][];
        Generations = new int[capacity][];
        _layerCapacities = new int[capacity];
        ColumnGenerations = new int[capacity];
        _columnCounts = new int[capacity];

        for (int i = 0; i < capacity; i++)
        {
            _nextSlots[i] = i == capacity - 1 ? int.MaxValue : i + 1;
            _nextFreeLayers[i] = 0;
            _layerCapacities[i] = layerCapacity;
            ColumnGenerations[i] = 1;
        }

        // There can be at most 'capacity' live column allocations initially. The
        // allocator grows its metadata and backing data automatically as required.
        int initialDataCapacity = checked(capacity * layerCapacity);
        _storage = layerCapacity == 0
            ? new FixedSpaceArray<T>(initialDataCapacity, capacity)
            : new FixedSpaceArray<T>(initialDataCapacity, capacity, layerCapacity);
    }

    /// <summary>Gets an allocated value using a generation-checked layer ID.</summary>
    public T this[LayerId id]
    {
        get
        {
            if (!IsAllocated(id))
                ThrowKeyNotFound();

            return GetColumnSpan(id.Index)[id.Layer];
        }
    }

    /// <summary>
    /// Gets a value without checking either generation or layer occupancy.
    /// The column must still be allocated; the ID must not be retained across mutations.
    /// </summary>
    public T this[UnsafeLayerId id] => GetColumnSpan(id.Index)[id.Layer];

    /// <summary>
    /// Allocates a new column and places its first value at <paramref name="startLayer"/>.
    /// </summary>
    public void AddColumn(T value, out LayerId id, int startLayer = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startLayer);

        if (_nextFree == int.MaxValue)
            ReAllocate();

        int columnIndex = _nextFree;
        int capacity = Math.Max(_layerCapacities[columnIndex], LayerCapacity);
        if (startLayer >= capacity)
            capacity = checked(startLayer + 1);

        EnsureLayerMetadataCapacity(columnIndex, capacity);
        _storage.AddSupportedCapacity(capacity);
        var column = _storage.AllocateSpan(capacity, out var allocationId);

        // Claim the stable Gen2D column index only after its backing allocation succeeds.
        _nextFree = _nextSlots[columnIndex];
        _nextSlots[columnIndex] = -1;
        _columnAllocations[columnIndex] = allocationId;

        InitializeLayerFreeList(columnIndex, capacity);
        RemoveLayerFromFreeList(columnIndex, startLayer);
        _nextSlotLayers[columnIndex][startLayer] = -1;

        _columnCounts[columnIndex] = 1;
        Count++;
        ColumnCount++;

        column[startLayer] = value;
        id = new LayerId(
            columnIndex,
            startLayer,
            Generations[columnIndex][startLayer],
            ColumnGenerations[columnIndex]);
    }

    /// <summary>Adds a value to the first free layer in an existing column.</summary>
    public void Add(T value, ColumnHandle index, out LayerId id)
    {
        int columnIndex = index.Index;
        if ((uint)columnIndex >= (uint)Length || _nextSlots[columnIndex] >= 0)
            ThrowKeyNotFound();

        int layer = _nextFreeLayers[columnIndex];
        if (layer == int.MaxValue)
        {
            int newCapacity = checked(Math.Max(_layerCapacities[columnIndex], 2) * 2);
            EnsureColumnLayerCapacity(columnIndex, newCapacity);
            layer = _nextFreeLayers[columnIndex];
        }

        _nextFreeLayers[columnIndex] = _nextSlotLayers[columnIndex][layer];
        _nextSlotLayers[columnIndex][layer] = -1;

        Count++;
        _columnCounts[columnIndex]++;

        id = new LayerId(
            columnIndex,
            layer,
            Generations[columnIndex][layer],
            ColumnGenerations[columnIndex]);
        GetColumnSpan(columnIndex)[layer] = value;
    }

    /// <summary>Overwrites an allocated element without changing its generation.</summary>
    public void Set(LayerId id, T value)
    {
        if (!IsAllocated(id))
            ThrowKeyNotFound();

        GetColumnSpan(id.Index)[id.Layer] = value;
    }

    /// <summary>Overwrites an element without generation or occupancy checks.</summary>
    public void SetUnsafe(UnsafeLayerId id, T value)
    {
        GetColumnSpan(id.Index)[id.Layer] = value;
    }

    /// <summary>Frees one allocated layer.</summary>
    public void Free(LayerId id)
    {
        if (!IsAllocated(id))
            ThrowKeyNotFound();

        FreeLayerCore(id.Index, id.Layer);
    }

    /// <summary>Frees one layer without generation or occupancy checks.</summary>
    public void FreeUnsafe(UnsafeLayerId id)
    {
        FreeLayerCore(id.Index, id.Layer);
    }

    /// <summary>Frees an entire column and invalidates all LayerIds referring to it.</summary>
    public void Free(ColumnHandle id)
    {
        int columnIndex = id.Index;
        if ((uint)columnIndex >= (uint)Length || _nextSlots[columnIndex] >= 0)
            ThrowKeyNotFound();

        if (!_storage.Free(_columnAllocations[columnIndex]))
            throw new InvalidOperationException("The backing column allocation is no longer active.");

        ColumnGenerations[columnIndex]++;
        Count -= _columnCounts[columnIndex];
        ColumnCount--;
        _columnCounts[columnIndex] = 0;

        _nextSlots[columnIndex] = _nextFree;
        _nextFree = columnIndex;
    }

    /// <summary>
    /// Creates a safe LayerId for a position in an allocated column.
    /// The layer need not currently contain an element, matching the original API's behavior.
    /// </summary>
    public LayerId GetLayerId(ColumnHandle idx, int layer)
    {
        int columnIndex = idx.Index;
        if (!idx.IsValid
            || (uint)columnIndex >= (uint)Length
            || _nextSlots[columnIndex] >= 0
            || (uint)layer >= (uint)_layerCapacities[columnIndex])
        {
            ThrowKeyNotFound();
        }

        return new LayerId(
            columnIndex,
            layer,
            Generations[columnIndex][layer],
            ColumnGenerations[columnIndex]);
    }

    /// <summary>Returns the first free layer, or int.MaxValue if the column is full.</summary>
    public int GetFreeLayer(ColumnHandle idx)
    {
        int columnIndex = idx.Index;
        if ((uint)columnIndex >= (uint)Length || _nextSlots[columnIndex] >= 0)
            ThrowKeyNotFound();

        return _nextFreeLayers[columnIndex];
    }

    /// <summary>Appends all currently allocated values in a column to <paramref name="list"/>.</summary>
    public void GetColumnData(ColumnHandle idx, List<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);

        int columnIndex = idx.Index;
        if ((uint)columnIndex >= (uint)Length || _nextSlots[columnIndex] >= 0)
            ThrowKeyNotFound();

        Span<T> column = GetColumnSpan(columnIndex);
        for (int layer = 0; layer < _layerCapacities[columnIndex]; layer++)
        {
            if (_nextSlotLayers[columnIndex][layer] == -1)
                list.Add(column[layer]);
        }
    }

    /// <summary>Tries to get a value using a generation-checked ID.</summary>
    public bool TryGetValue(LayerId id, [MaybeNullWhen(false)] out T value)
    {
        if (!IsAllocated(id))
        {
            value = default;
            return false;
        }

        value = GetColumnSpan(id.Index)[id.Layer];
        return true;
    }

    /// <summary>
    /// Tries to get a value without checking the layer generation or occupancy.
    /// Returns false if the column is no longer allocated or the coordinates are out of range.
    /// </summary>
    public bool TryGetValueUnsafe(UnsafeLayerId id, [MaybeNullWhen(false)] out T value)
    {
        if ((uint)id.Index >= (uint)Length
            || _nextSlots[id.Index] >= 0
            || (uint)id.Layer >= (uint)_layerCapacities[id.Index])
        {
            value = default;
            return false;
        }

        value = GetColumnSpan(id.Index)[id.Layer];
        return true;
    }

    /// <summary>Ensures that the column metadata capacity is at least <paramref name="capacity"/>.</summary>
    /// <returns>The resulting column metadata capacity.</returns>
    public int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity <= Length)
            return Length;

        ReAllocateTo(Math.Max(checked(Math.Max(Length, 2) * 2), capacity));
        return Length;
    }

    /// <summary>Ensures that every column has at least the specified capacity.</summary>
    public void EnsureLayerCapacity(int layerCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(layerCapacity);
        if (layerCapacity <= LayerCapacity)
            return;

        for (int i = 0; i < Length; i++)
        {
            if (_nextSlots[i] < 0)
                EnsureColumnLayerCapacity(i, layerCapacity);
            else
                _layerCapacities[i] = Math.Max(_layerCapacities[i], layerCapacity);
        }

        LayerCapacity = layerCapacity;
    }

    private bool IsAllocated(LayerId id)
    {
        if ((uint)id.Index >= (uint)Length
            || (uint)id.Layer >= (uint)_layerCapacities[id.Index])
        {
            return false;
        }

        return _nextSlots[id.Index] < 0
            && ColumnGenerations[id.Index] == id.ColumnGeneration
            && Generations[id.Index][id.Layer] == id.Generation
            && _nextSlotLayers[id.Index][id.Layer] == -1;
    }

    private Span<T> GetColumnSpan(int columnIndex)
    {
        return _storage.GetSpan(_columnAllocations[columnIndex]);
    }

    private void FreeLayerCore(int columnIndex, int layer)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            GetColumnSpan(columnIndex)[layer] = default!;

        Count--;
        _columnCounts[columnIndex]--;
        Generations[columnIndex][layer]++;

        _nextSlotLayers[columnIndex][layer] = _nextFreeLayers[columnIndex];
        _nextFreeLayers[columnIndex] = layer;
    }

    private void EnsureLayerMetadataCapacity(int columnIndex, int newCapacity)
    {
        int currentCapacity = _layerCapacities[columnIndex];
        int targetCapacity = Math.Max(currentCapacity, newCapacity);

        if (Generations[columnIndex] is null)
            Generations[columnIndex] = new int[targetCapacity];
        else if (Generations[columnIndex].Length < targetCapacity)
            Array.Resize(ref Generations[columnIndex], targetCapacity);

        if (_nextSlotLayers[columnIndex] is null)
            _nextSlotLayers[columnIndex] = new int[targetCapacity];
        else if (_nextSlotLayers[columnIndex].Length < targetCapacity)
            Array.Resize(ref _nextSlotLayers[columnIndex], targetCapacity);

        _layerCapacities[columnIndex] = targetCapacity;
    }

    private void InitializeLayerFreeList(int columnIndex, int capacity)
    {
        for (int layer = 0; layer < capacity; layer++)
        {
            Generations[columnIndex][layer] = 1;
            _nextSlotLayers[columnIndex][layer] =
                layer == capacity - 1 ? int.MaxValue : layer + 1;
        }

        _nextFreeLayers[columnIndex] = 0;
    }

    private void RemoveLayerFromFreeList(int columnIndex, int targetLayer)
    {
        int previous = -1;
        int current = _nextFreeLayers[columnIndex];

        while (current != int.MaxValue && current != targetLayer)
        {
            previous = current;
            current = _nextSlotLayers[columnIndex][current];
        }

        if (current != targetLayer)
            throw new InvalidOperationException("The requested initial layer is not in the free-layer list.");

        if (previous == -1)
            _nextFreeLayers[columnIndex] = _nextSlotLayers[columnIndex][current];
        else
            _nextSlotLayers[columnIndex][previous] = _nextSlotLayers[columnIndex][current];
    }

    private void EnsureColumnLayerCapacity(int columnIndex, int newCapacity)
    {
        int oldCapacity = _layerCapacities[columnIndex];
        if (newCapacity <= oldCapacity)
            return;

        _storage.AddSupportedCapacity(newCapacity);

        var oldAllocationId = _columnAllocations[columnIndex];
        var newColumn = _storage.AllocateSpan(newCapacity, out var newAllocationId);
        // A reused block can contain stale value-type data. Array.Resize in the
        // original implementation zero-initialized the new tail, so do that here too.
        newColumn.Clear();
        _storage.GetSpan(oldAllocationId).CopyTo(newColumn);

        if (!_storage.Free(oldAllocationId))
            throw new InvalidOperationException("The previous backing column allocation is no longer active.");

        _columnAllocations[columnIndex] = newAllocationId;

        Array.Resize(ref Generations[columnIndex], newCapacity);
        Array.Resize(ref _nextSlotLayers[columnIndex], newCapacity);

        for (int layer = oldCapacity; layer < newCapacity; layer++)
        {
            _nextSlotLayers[columnIndex][layer] =
                layer == newCapacity - 1 ? _nextFreeLayers[columnIndex] : layer + 1;
            Generations[columnIndex][layer] = 1;
        }

        _nextFreeLayers[columnIndex] = oldCapacity;
        _layerCapacities[columnIndex] = newCapacity;
    }

    private void ReAllocate()
    {
        int newLength = checked(Math.Max(Length, 2) * 2);
        ReAllocateTo(newLength);
    }

    private void ReAllocateTo(int newSize)
    {
        int oldLength = Length;
        Debug.Assert(newSize >= oldLength, "Cannot shrink Gen2DStorage.");
        if (newSize == oldLength)
            return;

        int previousFreeHead = _nextFree;

        Array.Resize(ref _nextSlots, newSize);
        Array.Resize(ref _columnAllocations, newSize);
        Array.Resize(ref _nextFreeLayers, newSize);
        Array.Resize(ref _nextSlotLayers, newSize);
        Array.Resize(ref Generations, newSize);
        Array.Resize(ref _layerCapacities, newSize);
        Array.Resize(ref ColumnGenerations, newSize);
        Array.Resize(ref _columnCounts, newSize);

        int tail = oldLength == 0 ? int.MaxValue : previousFreeHead;
        for (int i = oldLength; i < newSize; i++)
        {
            _layerCapacities[i] = LayerCapacity;
            _nextSlots[i] = i == newSize - 1 ? tail : i + 1;
            _nextFreeLayers[i] = 0;
            ColumnGenerations[i] = 1;
            _columnCounts[i] = 0;
        }

        _nextFree = oldLength;
        Length = newSize;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowKeyNotFound()
    {
        throw new KeyNotFoundException();
    }
}

/// <summary>
/// A generational identifier that uniquely references
/// a specific element in the <see cref="Gen2DStorage{T}"/>.
/// </summary>
public readonly record struct LayerId(int Index, int Layer, int Generation, int ColumnGeneration)
{
    /// <summary>
    /// An invalid index.
    /// </summary>
    public static readonly LayerId Invalid = new(-1, -1, 0, 0);

    /// <summary>
    /// Gets a <see cref="ColumnHandle"/> for this layer ID.
    /// </summary>
    public ColumnHandle ColumnHandle => new(Index);

    /// <summary>
    /// Gets a <see cref="UnsafeLayerId"/> for this layer ID.
    /// </summary>
    public UnsafeLayerId UnsafeId => new(Index, Layer);
    
    /// <summary>
    /// Checks whether a specified <see cref="GenId"/> is valid.
    /// </summary>
    public bool IsValid => Generation > 0 && ColumnGeneration > 0 && Index >= 0 && Layer >= 0;

    /// <inheritdoc/>
    public override string ToString() => $"[Col: {Index}, Layer: {Layer}, Gen: {Generation}, ColGen: {ColumnGeneration}]";
}

/// <summary>
/// A generational identifier that uniquely references
/// a specific element in the <see cref="Gen2DStorage{T}"/>.
/// Doesn't have any safety checks, and therefore it should never be used after the storage was mutated.
/// </summary>
public readonly record struct UnsafeLayerId(int Index, int Layer)
{
    /// <summary>
    /// An invalid index.
    /// </summary>
    public static readonly UnsafeLayerId Invalid = new(-1, -1);

    /// <summary>
    /// Gets a <see cref="ColumnHandle"/> for this layer ID.
    /// </summary>
    public ColumnHandle ColumnHandle => new(Index);
    
    /// <summary>
    /// Checks whether a specified <see cref="GenId"/> is valid.
    /// </summary>
    public bool IsValid => Index >= 0 && Layer >= 0;

    /// <inheritdoc/>
    public override string ToString() => $"[Col: {Index}, Layer: {Layer}]";
    
    /// <summary>
    /// Converts this unsafe layer ID into a safe version.
    /// </summary>
    public LayerId ToSafeId<T>(Gen2DStorage<T> storage) => new(Index, Layer, storage.Generations[Index][Layer], storage.ColumnGenerations[Layer]);
}

/// <summary>
/// Represents a reference to a specific column in <see cref="Gen2DStorage{T}"/>.
/// </summary>
public readonly record struct ColumnHandle(int Index)
{
    /// <summary>
    /// An invalid index.
    /// </summary>
    public static readonly ColumnHandle Invalid = new(-1);
    
    /// <summary>
    /// Checks whether a specified <see cref="ColumnHandle"/> is valid.
    /// </summary>
    public bool IsValid => Index >= 0;
}
