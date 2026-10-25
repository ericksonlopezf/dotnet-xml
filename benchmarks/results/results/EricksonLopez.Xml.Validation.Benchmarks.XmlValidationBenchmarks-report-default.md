
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3


 Method                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error         | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
-------------------------- |---------- |---------- |--------------- |------------ |------------ |-------------:|--------------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
 ValidateFromString        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  7,674.56 ns |    153.223 ns | 439.626 ns |     ? |       ? | 4.5013 | 0.8850 |   75680 B |           ? |
 ValidateFromStream        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  9,342.91 ns |    180.922 ns | 447.195 ns |     ? |       ? | 6.4697 | 1.6022 |  108528 B |           ? |
 ValidateFromSpan          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 10,173.12 ns |    214.031 ns | 631.075 ns |     ? |       ? | 6.4697 | 1.6022 |  108616 B |           ? |
 CacheLookupContainsSchema | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     11.63 ns |      0.031 ns |   0.026 ns |     ? |       ? |      - |      - |         - |           ? |
 ValidateFromString        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  8,409.80 ns |  5,022.486 ns | 275.299 ns |     ? |       ? | 4.5013 | 0.8926 |   75680 B |           ? |
 ValidateFromStream        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  9,702.50 ns | 10,663.449 ns | 584.500 ns |     ? |       ? | 6.4697 | 1.6022 |  108528 B |           ? |
 ValidateFromSpan          | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  9,907.13 ns |  2,536.325 ns | 139.025 ns |     ? |       ? | 6.4697 | 1.6022 |  108616 B |           ? |
 CacheLookupContainsSchema | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     11.63 ns |      0.357 ns |   0.020 ns |     ? |       ? |      - |      - |         - |           ? |

Benchmarks with issues:
  XmlValidationBenchmarks.ValidateFromString: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromString: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
