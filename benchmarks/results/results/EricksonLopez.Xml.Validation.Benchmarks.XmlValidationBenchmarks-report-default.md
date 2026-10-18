
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3


 Method                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean        | Error        | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
-------------------------- |---------- |---------- |--------------- |------------ |------------ |------------:|-------------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
 ValidateFromString        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 7,182.37 ns |   127.737 ns | 187.236 ns |     ? |       ? | 4.5013 | 0.8926 |   75680 B |           ? |
 ValidateFromStream        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 8,018.80 ns |    65.721 ns |  58.260 ns |     ? |       ? | 6.4697 | 1.6022 |  108528 B |           ? |
 ValidateFromSpan          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 8,078.42 ns |   131.596 ns | 123.095 ns |     ? |       ? | 6.4697 | 1.6022 |  108616 B |           ? |
 CacheLookupContainsSchema | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    11.64 ns |     0.021 ns |   0.018 ns |     ? |       ? |      - |      - |         - |           ? |
 ValidateFromString        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 6,959.46 ns | 2,599.836 ns | 142.506 ns |     ? |       ? | 4.5013 | 0.8926 |   75680 B |           ? |
 ValidateFromStream        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 8,512.04 ns | 3,647.634 ns | 199.939 ns |     ? |       ? | 6.4697 | 1.6022 |  108528 B |           ? |
 ValidateFromSpan          | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 8,377.69 ns | 2,146.768 ns | 117.672 ns |     ? |       ? | 6.4697 | 1.6022 |  108616 B |           ? |
 CacheLookupContainsSchema | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    11.53 ns |     0.587 ns |   0.032 ns |     ? |       ? |      - |      - |         - |           ? |

Benchmarks with issues:
  XmlValidationBenchmarks.ValidateFromString: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromString: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
