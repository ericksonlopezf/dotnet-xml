```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


```
| Method                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean        | Error        | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |---------- |---------- |--------------- |------------ |------------ |------------:|-------------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| ValidateFromString        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 6,717.53 ns |   110.482 ns |  86.257 ns |     ? |       ? | 4.5013 | 0.8926 |   75671 B |           ? |
| ValidateFromStream        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 8,012.28 ns |   157.061 ns | 235.081 ns |     ? |       ? | 6.4697 | 1.6022 |  108520 B |           ? |
| ValidateFromSpan          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 8,272.57 ns |   165.135 ns | 429.208 ns |     ? |       ? | 6.4697 | 1.6022 |  108608 B |           ? |
| CacheLookupContainsSchema | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    11.62 ns |     0.022 ns |   0.021 ns |     ? |       ? |      - |      - |         - |           ? |
| ValidateFromString        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromStream        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromSpan          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| CacheLookupContainsSchema | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromString        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromStream        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromSpan          | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| CacheLookupContainsSchema | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |          NA |           NA |         NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromString        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 7,075.54 ns | 2,420.309 ns | 132.665 ns |     ? |       ? | 4.5013 | 0.8926 |   75671 B |           ? |
| ValidateFromStream        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 8,419.93 ns | 1,761.286 ns |  96.542 ns |     ? |       ? | 6.4697 | 1.6022 |  108520 B |           ? |
| ValidateFromSpan          | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 8,836.97 ns | 7,237.396 ns | 396.706 ns |     ? |       ? | 6.4697 | 1.6022 |  108608 B |           ? |
| CacheLookupContainsSchema | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    11.67 ns |     1.142 ns |   0.063 ns |     ? |       ? |      - |      - |         - |           ? |

Benchmarks with issues:
  XmlValidationBenchmarks.ValidateFromString: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromString: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
