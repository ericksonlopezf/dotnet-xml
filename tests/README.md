# Testing Architecture & Quality Gates — EricksonLopez.Xml.Validation

Comprehensive guide to the test harness, test taxonomy, execution workflows, and quality gates for `EricksonLopez.Xml.Validation`.

---

## 1. Testing Projects Structure

The testing layer is organized into segregated projects adhering to the Testing Pyramid:

| Project | Type | Purpose | Key Frameworks & Tools |
|---|---|---|---|
| [`EricksonLopez.Xml.Validation.Tests`](./EricksonLopez.Xml.Validation.Tests) | Unit & Integration | Functional correctness, Anti-XXE enforcement, error mapping, concurrency, and DI lifecycle. | xUnit, AwesomeAssertions, Coverlet |
| [`EricksonLopez.Xml.Validation.ArchitectureTests`](./EricksonLopez.Xml.Validation.ArchitectureTests) | Architectural | Boundary isolation, sealed classes, namespace invariants, zero `[Obsolete]` APIs. | NetArchTest.Rules, AwesomeAssertions |
| [`EricksonLopez.Xml.Validation.AotTest`](./EricksonLopez.Xml.Validation.AotTest) | Smoke Test (Native AOT) | Native ahead-of-time compilation and trimming safety verification on runtime executables. | .NET Native AOT CLI |
| [`EricksonLopez.Xml.Validation.Benchmarks`](../benchmarks/EricksonLopez.Xml.Validation.Benchmarks) | Performance | Microbenchmarks assessing allocations and throughput across string, stream, and span validation. | BenchmarkDotNet |

---

## 2. Test Conventions & Design Patterns

### 2.1 Naming Standard
Tests follow the standard BDD-style naming convention:
```
MethodName_Scenario_ExpectedResult
```
*Examples:*
- `Validate_ValidXml_ReturnsSuccess`
- `Validate_MalformedXml_ReturnsMalformedError`
- `ValidateAsync_CancelledDuringReadLoop_ThrowsOperationCanceledException`

### 2.2 Centralized Test Fixtures (`Common/`)
Shared test constants, payloads, and test doubles are isolated in [`EricksonLopez.Xml.Validation.Tests/Common`](./EricksonLopez.Xml.Validation.Tests/Common):
- `XmlTestSamples`: Centralized XSD schemas and XML payloads (happy path, invalid, warnings, Anti-XXE, schemaLocation).
- `TestLogger<T>`: Thread-safe spy logger capturing log entries without third-party mocking frameworks.
- `NullSchemaCacheMock`: Test double for defensive cache validation testing.
- `SlowAsyncStream`: Deterministic async stream for testing cancellation token loops without flakiness or timers.

### 2.3 Test Execution Strategy & Mutation Tracer Compatibility
Test execution is configured via `xunit.runner.json`:
```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false,
  "maxParallelThreads": 1
}
```
Sequential execution across collections guarantees full deterministic compatibility with Stryker.NET's per-test coverage tracer (`CoverageBasedTest` mode), preventing coverage loss while completing the entire in-memory test suite in under 450 ms.

---

## 3. Running Tests Locally

### 3.1 Complete Test Suite Execution
Execute all unit and architecture tests across all target frameworks (`net8.0`, `net9.0`, `net10.0`):
```bash
dotnet test --configuration Release
```

### 3.2 Architecture Tests Only
Verify architectural rules and package boundary constraints:
```bash
dotnet test tests/EricksonLopez.Xml.Validation.ArchitectureTests/EricksonLopez.Xml.Validation.ArchitectureTests.csproj
```

### 3.3 Native AOT Smoke Test
Compile and run the AOT verification console application:
```bash
dotnet run --project tests/EricksonLopez.Xml.Validation.AotTest/EricksonLopez.Xml.Validation.AotTest.csproj --configuration Release
```

To test full ahead-of-time publishing as a standalone binary:
```bash
dotnet publish tests/EricksonLopez.Xml.Validation.AotTest/EricksonLopez.Xml.Validation.AotTest.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained \
  --output ./aot-bin

./aot-bin/EricksonLopez.Xml.Validation.AotTest.exe
```

### 3.4 Mutation Testing with Stryker.NET
Analyze test sensitivity and verify mutation coverage against quality gates:
```bash
dotnet stryker --config-file stryker-config.json
```

Quality thresholds enforced:
- **High**: `100%`
- **Low**: `98%`
- **Break**: `95%` (Build fails if score is < 95%)
