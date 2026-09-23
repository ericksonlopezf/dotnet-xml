# Contributing to EricksonLopez.Xml.Validation

Thank you for your interest in contributing to `EricksonLopez.Xml.Validation`! This document outlines the technical standards, development workflow, and pull request guidelines for the repository.

---

## 1. Code of Conduct

All contributors and maintainers are expected to abide by our [Code of Conduct](CODE_OF_CONDUCT.md). Please read it before participating.

---

## 2. Prerequisites

To build, test, and run the projects in this repository, you will need:

- **.NET SDK:** .NET 10.0 SDK (`10.0.100` pinned in [`global.json`](global.json)), with .NET 8.0 and .NET 9.0 runtimes installed locally for cross-TFM execution. (Library targets `net8.0;net9.0;net10.0`).
- **Operating System:** Windows, Linux, or macOS.
- **IDE:** Visual Studio 2022+ (v17.12+) / VS Code / JetBrains Rider (2024.3+).

---

## 3. Solution Structure

The repository solution is defined in [`EricksonLopez.Xml.slnx`](EricksonLopez.Xml.slnx):

```text
dotnet-xml/
├── src/
│   └── EricksonLopez.Xml.Validation/                   # Core library package (packable, net8.0;net9.0;net10.0)
├── samples/
│   └── EricksonLopez.Xml.Showcase/                     # 11-level reference implementation (net10.0)
├── tests/
│   ├── EricksonLopez.Xml.Validation.Tests/             # Unit and integration test suite (net8.0;net9.0;net10.0)
│   ├── EricksonLopez.Xml.Validation.ArchitectureTests/ # NetArchTest architecture guardrail suite (net8.0;net9.0;net10.0)
│   └── EricksonLopez.Xml.Validation.AotTest/           # Native AOT smoke verification project (net10.0)
├── benchmarks/
│   └── EricksonLopez.Xml.Validation.Benchmarks/        # BenchmarkDotNet performance and allocation suite (net10.0)
├── docs/                                               # Technical documentation, guides, and 17 ADRs
├── scripts/                                            # Compliance, mutation gate, and benchmark regression scripts
├── Directory.Build.props                               # Central MSBuild properties, warnings, and AOT analyzers
└── Directory.Packages.props                            # Central Package Management (CPM)
```

---

## 4. Build and Test Commands

### 4.1 Restore and Build

Restore and compile the entire solution in Release configuration:

```bash
dotnet restore EricksonLopez.Xml.slnx
dotnet build EricksonLopez.Xml.slnx -c Release --no-restore
```

> [!NOTE]
> `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` is enforced across all projects with `<WarningLevel>5</WarningLevel>`. Any compiler warning fails the build.

### 4.2 Run Test Suites

Run unit and integration tests across all target frameworks:

```bash
dotnet test EricksonLopez.Xml.slnx -c Release --no-build
```

Run test suite with code coverage collection:

```bash
dotnet test EricksonLopez.Xml.slnx -c Release --no-build --collect:"XPlat Code Coverage"
```

Run architecture tests:

```bash
dotnet test tests/EricksonLopez.Xml.Validation.ArchitectureTests/EricksonLopez.Xml.Validation.ArchitectureTests.csproj -c Release
```

### 4.3 Verify Native AOT Compatibility

Run the Native AOT verification smoke test executable:

```bash
dotnet run --project tests/EricksonLopez.Xml.Validation.AotTest/EricksonLopez.Xml.Validation.AotTest.csproj -c Release
```

Or execute native publishing directly (requires Linux with `clang`, `lld`, `zlib`):

```bash
dotnet publish tests/EricksonLopez.Xml.Validation.AotTest/EricksonLopez.Xml.Validation.AotTest.csproj -c Release -r linux-x64 --self-contained -o ./aot-output
./aot-output/EricksonLopez.Xml.Validation.AotTest
```

### 4.4 Run Mutation Testing (Stryker.NET)

Restore local tools and execute mutation testing:

```bash
dotnet tool restore
dotnet stryker --config-file stryker-config.json
```

### 4.5 Run Performance Benchmarks & Regression Gate

Execute the BenchmarkDotNet suite:

```bash
dotnet run --project benchmarks/EricksonLopez.Xml.Validation.Benchmarks/EricksonLopez.Xml.Validation.Benchmarks.csproj -c Release --framework net10.0 -- --filter "*" --job short --memory
```

Evaluate benchmark results against baseline:

```powershell
powershell -ExecutionPolicy Bypass -File ./scripts/verify-benchmark-gate.ps1 -ReportDir ./benchmarks/pr-results -BaselinePath ./benchmarks/results/baseline.json -MaxLatencyRegressionPercent 5
```

### 4.6 Run Repository Compliance Verifier

Audit repository governance, naming conventions, and quality gates locally:

```powershell
powershell -ExecutionPolicy Bypass -File ./scripts/verify-compliance.ps1
```

### 4.7 Run the Interactive Showcase

Run all 11 progressive levels of the showcase:

```bash
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj
```

Or run an individual showcase level (e.g., Level 3 for enterprise schemas):

```bash
dotnet run --project samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj -- 3
```

---

## 5. Architectural Invariants

Every contribution must preserve the core design invariants:

1. **Anti-XXE Enforced by Default:** `DtdProcessing.Prohibit` and `XmlResolver = null` must be applied unconditionally on all XML reading and schema compiling code paths. There is no escape hatch or public opt-out parameter (see [ADR-001](docs/adr/adr-001-anti-xxe-by-default.md)).
2. **Result Pattern over Exceptions:** Validation failures, malformed XML, and missing schemas must return structured `Result<bool>` failures (`Error.Validation` or `Error.NotFound`), never throwing exceptions for control flow (see [ADR-002](docs/adr/adr-002-result-instead-of-exceptions.md)).
3. **Thread Safety & Immutability:** `XmlSchemaCache` and `XmlSchemaValidator` are registered as singletons and must remain completely thread-safe under concurrent read/write workloads.
4. **Native AOT & Trimming Safety:** Maintain `<IsAotCompatible>true</IsAotCompatible>` and `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>`. No runtime reflection, no dynamic dispatch, and no unanalyzed IL emit.
5. **Zero-Allocation Logging:** Structured diagnostic logging must use compile-time source-generated `[LoggerMessage]` partial methods.

---

## 6. Branching and Commit Conventions

### Branch Naming
- `feature/<short-description>`
- `fix/<short-description>`
- `docs/<short-description>`
- `refactor/<short-description>`
- `perf/<short-description>`

### Commit Messages (Conventional Commits)
All commit messages must follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

- `feat(validation): add span-based overload`
- `fix(reader): set Async = true on XmlReaderSettings`
- `docs(adr): document warning configuration rationale`
- `test(cache): add concurrent directory scan tests`
- `perf(cache): optimize dictionary lookup for root elements`

---

## 7. Pull Request Checklist

Before submitting a pull request, ensure the following quality checklist is satisfied:

- [ ] **Build**: `dotnet build EricksonLopez.Xml.slnx -c Release` completes with 0 errors and 0 warnings (`TreatWarningsAsErrors` enabled).
- [ ] **Automated Tests**: `dotnet test EricksonLopez.Xml.slnx -c Release` passes across .NET 8.0, 9.0, and 10.0.
- [ ] **Architecture Rules**: NetArchTest suite passes (`EricksonLopez.Xml.Validation.ArchitectureTests`).
- [ ] **Native AOT & Trimming**: No trimming warnings; AOT smoke test publishes and runs cleanly (`tests/EricksonLopez.Xml.Validation.AotTest`).
- [ ] **Security & Anti-XXE**: `DtdProcessing.Prohibit` and `XmlResolver = null` maintained across all code paths.
- [ ] **Error Handling**: Uses `Result<T>` from `EricksonLopez.Result` (zero exceptions for validation control flow).
- [ ] **Mutation Testing**: Meets the Tier-1 Stryker break threshold ($\ge 95\%$).
- [ ] **Performance Gate**: Zero heap allocations on hot paths and $\le 5\%$ latency variance vs baseline.
- [ ] **Repository Compliance**: `verify-compliance.ps1` passes with 0 violations.
- [ ] **Central Package Management**: `Directory.Packages.props` respected (no inline package versions in `.csproj`).
- [ ] **Documentation**: `CHANGELOG.md` updated and relevant `/docs/` guides modified or added.
- [ ] **Commits**: Follows [Conventional Commits](https://www.conventionalcommits.org/).
