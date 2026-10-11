
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


 Method                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean        | Error        | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
-------------------------- |---------- |---------- |--------------- |------------ |------------ |------------:|-------------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
 ValidateFromString        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 6,971.69 ns |   132.602 ns | 130.233 ns |     ? |       ? | 4.5013 | 0.8926 |   75671 B |           ? |
 ValidateFromStream        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 8,196.76 ns |   161.830 ns | 351.806 ns |     ? |       ? | 6.4697 | 1.6022 |  108520 B |           ? |
 ValidateFromSpan          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 8,253.25 ns |   164.498 ns | 354.099 ns |     ? |       ? | 6.4697 | 1.6022 |  108608 B |           ? |
 CacheLookupContainsSchema | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    11.88 ns |     0.115 ns |   0.102 ns |     ? |       ? |      - |      - |         - |           ? |
 ValidateFromString        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 6,882.02 ns |   843.406 ns |  46.230 ns |     ? |       ? | 4.5013 | 0.8926 |   75671 B |           ? |
 ValidateFromStream        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 8,293.58 ns | 5,886.333 ns | 322.650 ns |     ? |       ? | 6.4697 | 1.6022 |  108520 B |           ? |
 ValidateFromSpan          | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 7,995.41 ns |   975.292 ns |  53.459 ns |     ? |       ? | 6.4697 | 1.6022 |  108608 B |           ? |
 CacheLookupContainsSchema | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    11.65 ns |     0.716 ns |   0.039 ns |     ? |       ? |      - |      - |         - |           ? |

Benchmarks with issues:
  XmlValidationBenchmarks.ValidateFromString: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromString: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
