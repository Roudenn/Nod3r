using BenchmarkDotNet.Attributes;

namespace Nod3r.Collections.Benchmarks;

[MemoryDiagnoser]
public class FixedSpaceArrayBenchmark
{
    /// <summary>
    /// Amount of elements in every list.
    /// </summary>
    [Params(8, 64, 256)]
    public int Elements { get; set; }
    
    /// <summary>
    /// Amount of allocated lists.
    /// </summary>
    [Params(8, 64, 256)]
    public int Entries { get; set; }
    
    [Benchmark]
    public void AllocateAndReverse_FixedSpaceArray()
    {
        var storage = new FixedSpaceArray<int>(Entries * Elements, Entries, Elements);
        var ids = new List<GenId>(Entries);
        for (int i = 0; i < Entries; i++)
        {
            var list = storage.Allocate(Elements, out var id);
            for (int j = 0; j < Elements; j++)
            {
                list.Add(j);
            }
            
            ids.Add(id);
        }

        // TODO global enumerator for FixedSpaceArray
        foreach (var id in ids)
        {
            var list = storage.Get(id);
            list.Reverse();
        }
    }
    
    [Benchmark]
    public void AllocateAndReverse_NestedListArray()
    {
        var storage = new List<int>[Entries];
        for (int i = 0; i < Entries; i++)
        {
            storage[i] = new List<int>(Elements);
            for (int j = 0; j < Elements; j++)
            {
                storage[i].Add(j);
            }
        }

        foreach (var list in storage)
        {
            list.Reverse();
        }
    }
}

/// <summary>
/// Static class for simulating scattered memory conditions of the program.
/// Allows to detect cache misses more persuasively.
/// </summary>
public static class MemoryGarbageGenerator
{
    // ReSharper disable once CollectionNeverQueried.Local
    private static readonly List<object[]> Storage = new();
    
    public static void Allocate(int amount = 16)
    {
        var random = new Random(1234);
        Storage.Add(new object[Math.Max(4, random.Next(amount))]);
    }
}
