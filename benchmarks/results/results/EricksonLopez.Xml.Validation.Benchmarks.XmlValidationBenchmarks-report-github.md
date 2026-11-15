```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.69GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


```
| Method                    | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error         | StdDev      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |---------- |---------- |--------------- |------------ |------------ |-------------:|--------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| ValidateFromString        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 4,667.932 ns |    36.6555 ns |  30.6090 ns |     ? |       ? | 4.5013 | 0.8926 |   75680 B |           ? |
| ValidateFromStream        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 5,765.833 ns |   114.5476 ns | 148.9443 ns |     ? |       ? | 6.4697 | 1.6098 |  108528 B |           ? |
| ValidateFromSpan          | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 5,598.955 ns |   109.5772 ns | 112.5277 ns |     ? |       ? | 6.4697 | 1.6098 |  108616 B |           ? |
| CacheLookupContainsSchema | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     9.271 ns |     0.0132 ns |   0.0110 ns |     ? |       ? |      - |      - |         - |           ? |
| ValidateFromString        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromStream        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromSpan          | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| CacheLookupContainsSchema | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromString        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromStream        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromSpan          | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| CacheLookupContainsSchema | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |          NA |     ? |       ? |     NA |     NA |        NA |           ? |
| ValidateFromString        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 4,768.016 ns | 1,122.2003 ns |  61.5116 ns |     ? |       ? | 4.5013 | 0.8926 |   75680 B |           ? |
| ValidateFromStream        | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 5,588.811 ns | 2,799.7069 ns | 153.4614 ns |     ? |       ? | 6.4697 | 1.6098 |  108528 B |           ? |
| ValidateFromSpan          | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 5,542.865 ns | 2,812.5584 ns | 154.1658 ns |     ? |       ? | 6.4697 | 1.6098 |  108616 B |           ? |
| CacheLookupContainsSchema | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     9.277 ns |     0.1190 ns |   0.0065 ns |     ? |       ? |      - |      - |         - |           ? |

Benchmarks with issues:
  XmlValidationBenchmarks.ValidateFromString: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  XmlValidationBenchmarks.ValidateFromString: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromStream: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.ValidateFromSpan: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  XmlValidationBenchmarks.CacheLookupContainsSchema: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
