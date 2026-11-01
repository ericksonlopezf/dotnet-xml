```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 3.21GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3


```
| Method                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean        | Error        | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |---------- |---------- |--------------- |------------ |------------ |------------:|-------------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| ValidateFromString        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 6,223.20 ns |    15.571 ns |  13.803 ns |     ? |       ? | 4.5013 | 0.8926 |   75680 B |           ? |
| ValidateFromStream        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 7,534.18 ns |   150.710 ns | 154.768 ns |     ? |       ? | 6.4697 | 1.6098 |  108528 B |           ? |
| ValidateFromSpan          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 7,683.84 ns |    19.652 ns |  18.383 ns |     ? |       ? | 6.4697 | 1.6022 |  108616 B |           ? |
| CacheLookupContainsSchema | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    11.61 ns |     0.008 ns |   0.007 ns |     ? |       ? |      - |      - |         - |           ? |
| ValidateFromString        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromStream        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromSpan          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| CacheLookupContainsSchema | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromString        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromStream        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromSpan          | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| CacheLookupContainsSchema | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromString        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 6,580.79 ns |   447.509 ns |  24.529 ns |     ? |       ? | 4.5013 | 0.8926 |   75680 B |           ? |
| ValidateFromStream        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 7,440.07 ns | 1,668.159 ns |  91.437 ns |     ? |       ? | 6.4697 | 1.6098 |  108528 B |           ? |
| ValidateFromSpan          | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 7,792.40 ns |    87.616 ns |   4.803 ns |     ? |       ? | 6.4697 | 1.6022 |  108616 B |           ? |
| CacheLookupContainsSchema | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    11.62 ns |     0.051 ns |   0.003 ns |     ? |       ? |      - |      - |         - |           ? |

Benchmarks with issues:
  XmlValidationBenchmarks.ValidateFromString: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromString: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
