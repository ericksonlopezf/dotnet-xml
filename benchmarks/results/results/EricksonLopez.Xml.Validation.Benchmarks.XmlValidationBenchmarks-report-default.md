
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


 Method                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean        | Error        | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
-------------------------- |---------- |---------- |--------------- |------------ |------------ |------------:|-------------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
 ValidateFromString        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 7,736.52 ns |   154.371 ns | 442.920 ns |     ? |       ? | 4.5013 | 0.8926 |   75672 B |           ? |
 ValidateFromStream        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 9,071.01 ns |   179.781 ns | 290.313 ns |     ? |       ? | 6.4697 | 1.6022 |  108520 B |           ? |
 ValidateFromSpan          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 8,711.99 ns |   170.823 ns | 196.720 ns |     ? |       ? | 6.4697 | 1.6022 |  108608 B |           ? |
 CacheLookupContainsSchema | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    11.64 ns |     0.041 ns |   0.036 ns |     ? |       ? |      - |      - |         - |           ? |
 ValidateFromString        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 8,169.65 ns | 4,321.526 ns | 236.877 ns |     ? |       ? | 4.5013 | 0.8850 |   75671 B |           ? |
 ValidateFromStream        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 9,093.31 ns | 1,157.070 ns |  63.423 ns |     ? |       ? | 6.4697 | 1.6022 |  108520 B |           ? |
 ValidateFromSpan          | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 9,094.22 ns | 1,465.846 ns |  80.348 ns |     ? |       ? | 6.4697 | 1.6022 |  108608 B |           ? |
 CacheLookupContainsSchema | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    11.56 ns |     0.066 ns |   0.004 ns |     ? |       ? |      - |      - |         - |           ? |

Benchmarks with issues:
  XmlValidationBenchmarks.ValidateFromString: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromString: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
