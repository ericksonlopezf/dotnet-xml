
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V45 4.47GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


 Method                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error         | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
-------------------------- |---------- |---------- |--------------- |------------ |------------ |-------------:|--------------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
 ValidateFromString        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 3,430.741 ns |    67.9616 ns | 83.4630 ns |     ? |       ? | 4.5013 | 0.8965 |   75680 B |           ? |
 ValidateFromStream        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 3,933.932 ns |    77.5400 ns | 76.1546 ns |     ? |       ? | 6.4697 | 1.6098 |  108528 B |           ? |
 ValidateFromSpan          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 3,914.820 ns |    37.5850 ns | 33.3181 ns |     ? |       ? | 6.4697 | 1.6098 |  108616 B |           ? |
 CacheLookupContainsSchema | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     6.131 ns |     0.0435 ns |  0.0364 ns |     ? |       ? |      - |      - |         - |           ? |
 ValidateFromString        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromStream        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromSpan          | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 CacheLookupContainsSchema | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
 ValidateFromString        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 3,289.478 ns | 1,719.8960 ns | 94.2733 ns |     ? |       ? | 4.5013 | 0.8965 |   75680 B |           ? |
 ValidateFromStream        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 3,872.620 ns |   116.3145 ns |  6.3756 ns |     ? |       ? | 6.4697 | 1.6136 |  108528 B |           ? |
 ValidateFromSpan          | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 3,955.783 ns |   224.8338 ns | 12.3239 ns |     ? |       ? | 6.4697 | 1.6136 |  108616 B |           ? |
 CacheLookupContainsSchema | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     6.266 ns |     2.6960 ns |  0.1478 ns |     ? |       ? |      - |      - |         - |           ? |

Benchmarks with issues:
  XmlValidationBenchmarks.ValidateFromString: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromString: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
