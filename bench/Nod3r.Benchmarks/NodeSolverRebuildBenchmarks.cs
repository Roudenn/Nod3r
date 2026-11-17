using BenchmarkDotNet.Attributes;
using Nod3r.API;
using Nod3r.Examples;
using Numos.Maths;

namespace Nod3r.Benchmarks;

[MemoryDiagnoser]
public class NodeSolverRebuildBenchmarks
{
    /// <summary>
    /// Dimensions of the node block to spawn.
    /// </summary>
    [Params(4, 8, 16, 32)]
    public int Dimensions { get; set; }
    
    [Benchmark]
    public void Fill_Rebuild_AdjacentNode()
    {
        var world = new NodeWorld(reg =>
        {
            reg.Register<AdjacentNode, AdjacentNodeNet>();
        });
        var simulation = world.CreateSimulation(Dimensions, Dimensions, Dimensions);
        var handle = simulation.EnsureChunk<AdjacentNode>(Int3Helpers.Zero);
        for (int z = 0; z < Dimensions; z++)
        {
            for (int y = 0; y < Dimensions; y++)
            {
                for (int x = 0; x < Dimensions; x++)
                {
                    simulation.SetNode(new AdjacentNode(), handle, new Int3(y, x, z));
                }
            }
        }
        
        simulation.Rebuild();
    }

    [Benchmark]
    public void Lines_Rebuild_AdjacentNode()
    {
        var world = new NodeWorld(reg => { reg.Register<AdjacentNode, AdjacentNodeNet>(); });
        var simulation = world.CreateSimulation(Dimensions, Dimensions, Dimensions);
        var handle = simulation.EnsureChunk<AdjacentNode>(Int3Helpers.Zero);
        for (int z = 0; z < Dimensions; z++)
        {
            if (z % 2 != 0)
                continue;

            for (int y = 0; y < Dimensions; y++)
            {
                if (y % 2 != 0)
                    continue;

                for (int x = 0; x < Dimensions; x++)
                {
                    simulation.SetNode(new AdjacentNode(), handle, new Int3(y, x, z));
                }
            }
        }
        
        simulation.Rebuild();
    }
}
