using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Nod3r.Benchmarks;

internal static class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nWARNING: YOU ARE RUNNING A DEBUG BUILD, USE A RELEASE BUILD FOR AN ACCURATE BENCHMARK");
        Console.WriteLine("THE DEBUG BUILD IS ONLY GOOD FOR FIXING A CRASHING BENCHMARK\n");
        var baseConfig = new DebugInProcessConfig();
#endif
        var config = ManualConfig.Create(DefaultConfig.Instance);
        config.BuildTimeout = TimeSpan.FromMinutes(5);
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}
