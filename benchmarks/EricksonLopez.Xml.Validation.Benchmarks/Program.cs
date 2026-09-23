// Copyright © Erickson Lopez. MIT License.
using BenchmarkDotNet.Running;

namespace EricksonLopez.Xml.Validation.Benchmarks;

/// <summary>
/// Provides the entry point for the XML validation benchmark suite.
/// </summary>
public static class Program
{
    /// <summary>
    /// Executes the benchmark runner with the specified command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the runner</param>
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
