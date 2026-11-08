```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 7763 2.59GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3


```
| Method                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |---------- |---------- |--------------- |------------ |------------ |------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| ValidateFromString        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 6,404.40 ns |  32.567 ns |  28.870 ns |     ? |       ? | 4.5013 | 0.8926 |   75680 B |           ? |
| ValidateFromStream        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 7,470.95 ns | 148.705 ns | 165.285 ns |     ? |       ? | 6.4697 | 1.6098 |  108528 B |           ? |
| ValidateFromSpan          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 7,561.61 ns |  30.340 ns |  26.895 ns |     ? |       ? | 6.4697 | 1.6098 |  108616 B |           ? |
| CacheLookupContainsSchema | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    11.61 ns |   0.013 ns |   0.011 ns |     ? |       ? |      - |      - |         - |           ? |
| ValidateFromString        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |         NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromStream        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |         NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromSpan          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |         NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| CacheLookupContainsSchema | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |         NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromString        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |         NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromStream        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |         NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromSpan          | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |         NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| CacheLookupContainsSchema | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |         NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromString        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 6,396.09 ns |  58.813 ns |   3.224 ns |     ? |       ? | 4.5013 | 0.8926 |   75680 B |           ? |
| ValidateFromStream        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 7,327.67 ns | 318.894 ns |  17.480 ns |     ? |       ? | 6.4697 | 1.6098 |  108528 B |           ? |
| ValidateFromSpan          | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 7,188.38 ns |  91.454 ns |   5.013 ns |     ? |       ? | 6.4697 | 1.6098 |  108616 B |           ? |
| CacheLookupContainsSchema | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    11.61 ns |   0.198 ns |   0.011 ns |     ? |       ? |      - |      - |         - |           ? |

Benchmarks with issues:
  XmlValidationBenchmarks.ValidateFromString: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromString: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
