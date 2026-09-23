# Framework Testing Roadmap & Quality Audit — EricksonLopez.Xml.Validation

> **Canonical Document:** This document provides the authoritative quality assurance specification, test taxonomy, coverage verification, and Stryker mutation testing audit for `EricksonLopez.Xml.Validation`.

---

## Objectives

Elevate the `EricksonLopez.Xml.Validation` framework to a state of absolute software quality where **all framework behaviors and security invariants are comprehensively tested**, verified, and audited against mutation testing, adhering strictly to the following target metrics:

| Metric | Target | Initial State | Verified Final State | Status |
| :--- | :---: | :---: | :---: | :---: |
| **Line Coverage** | **≥ 100%** | 100% | **100.00% (260/260 lines)** | **COMPLIANT** |
| **Branch Coverage** | **≥ 100%** | 100% | **100.00% (46/46 branches)** | **COMPLIANT** |
| **Method Coverage** | **≥ 100%** | 100% | **100.00% (24/24 methods)** | **COMPLIANT** |
| **Mutation Score** | **100%** | 96.58% (4 survived) | **100.00% (117 killed / 0 survived)** | **COMPLIANT** |

Non-negotiable quality gate:
```text
Line Coverage    = 100%
Branch Coverage  = 100%
Method Coverage  = 100%
Mutation Score   = 100%
```

---

## Framework Architecture & Topology

The `EricksonLopez.Xml` library is a foundational (Tier 0) package dedicated to XML and XSD schema validation with Anti-XXE defense by default, Native AOT compilation compatibility, and zero-allocation streaming.

### Solution Project Graph (`EricksonLopez.Xml.slnx`)

- **Production Package:**
  - `src/EricksonLopez.Xml.Validation/EricksonLopez.Xml.Validation.csproj`
  - **Target Frameworks:** `net8.0;net9.0;net10.0`
  - **Core Dependencies:** `EricksonLopez.Result` (2.0.0), `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Options`.
  - **Compiler Properties:** `<IsAotCompatible>true</IsAotCompatible>`, `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`.

- **Verification and Testing Projects:**
  - `tests/EricksonLopez.Xml.Validation.Tests/EricksonLopez.Xml.Validation.Tests.csproj`: Comprehensive test suite for unit, asynchronous, concurrency, and dependency injection testing (xUnit, AwesomeAssertions, Coverlet).
  - `tests/EricksonLopez.Xml.Validation.ArchitectureTests/EricksonLopez.Xml.Validation.ArchitectureTests.csproj`: Architectural boundary enforcement and assembly invariants (NetArchTest.Rules).
  - `tests/EricksonLopez.Xml.Validation.AotTest/EricksonLopez.Xml.Validation.AotTest.csproj`: Native AOT executable smoke verification harness (`PublishAot: true`).
  - `benchmarks/EricksonLopez.Xml.Validation.Benchmarks/EricksonLopez.Xml.Validation.Benchmarks.csproj`: High-throughput performance and memory allocation benchmarking (BenchmarkDotNet).
  - `samples/EricksonLopez.Xml.Showcase/EricksonLopez.Xml.Showcase.csproj`: Comprehensive 11-level executable showcase demonstrating capabilities, integration patterns, and defensive architectures.

---

## Public API Specification

| Type / Symbol | Category | Contractual Responsibility | Architectural Invariants |
| :--- | :--- | :--- | :--- |
| `IXmlSchemaCache` | `PUBLIC_API / INTERFACE` | In-memory registration and caching of precompiled XSD schemas | Thread-safe, Anti-XXE (`XmlResolver = null`, `DtdProcessing = Prohibit`) |
| `XmlSchemaCache` | `PUBLIC_API / COMPONENT` | Concrete cache implementation backed by `ConcurrentDictionary<string, SchemaCacheEntry>` (each entry wraps a compiled `XmlSchemaSet` + a pre-indexed `HashSet` of global elements) | Safe compilation without external entity resolution, deterministic overwrite per namespace |
| `IXmlSchemaValidator` | `PUBLIC_API / INTERFACE` | Synchronous and asynchronous XSD schema validation engine | Functional return `Result<bool>`, `leaveOpen: true` on streams, Anti-XXE defaults |
| `XmlSchemaValidator` | `PUBLIC_API / COMPONENT` | Production validator with options support and zero-allocation structured logging | DTD prohibition, exception-free validation failure flow, configurable warning escalation |
| `XmlValidationOptions` | `PUBLIC_API / MODEL` | POCO validation configuration options | `IncludeWarnings` (default: false), `TreatWarningsAsErrors` (default: false) |
| `XmlValidationServiceCollectionExtensions` | `PUBLIC_API / EXTENSION` | Dependency injection container integration for Microsoft DI | Singleton registration of `IXmlSchemaCache` and `IXmlSchemaValidator` |

---

## Core Feature Capabilities

1. **Anti-XXE Enforcement By Default:**
   - All XML parsing pipelines and XSD schema compilation paths strictly configure `DtdProcessing = DtdProcessing.Prohibit` and `XmlResolver = null`.
2. **Precompiled XSD Schema Cache:**
   - XSD schemas are compiled once (`XmlSchemaSet.Compile()`) and cached in memory by `targetNamespace` to eliminate recurring compilation overhead in high-throughput pipelines.
3. **Structured `Result<bool>` Error Reporting:**
   - Schema validation failures and malformed documents never throw exceptions; they are mapped into structured `Error.NotFound` or `Error.Validation` descriptors from `EricksonLopez.Result`.
4. **Configurable Warning Handling:**
   - Options to include non-fatal schema warnings in validation error descriptors or escalate them to fatal failures via `TreatWarningsAsErrors`.
5. **Zero-Allocation Diagnostic Logging:**
   - Compile-time source generation via `[LoggerMessage]` (EventIds 1001–1005) for `ILogger<XmlSchemaValidator>`.
6. **Multi-Modal Input Support:**
   - Direct validation from `string`, `Stream`, `ReadOnlySpan<byte>` (native UTF-8 span), files, and directories.

---

## Component Test & Mutation Matrix

| Unit | Type | Status | Line Cov | Branch Cov | Method Cov | Mutation Score | Mutants (Killed / Surv) |
| :--- | :--- | :---: | ---: | ---: | ---: | ---: | :---: |
| `XmlSchemaCache` | `COMPONENT` | **DONE** | 100% | 100% | 100% | **100.00%** | 42 / 0 |
| `XmlSchemaValidator` | `COMPONENT` | **DONE** | 100% | 100% | 100% | **100.00%** | 70 / 0 |
| `XmlValidationOptions` | `COMPONENT` | **DONE** | 100% | 100% | 100% | **N/A** | POCO without branching |
| `XmlValidationServiceCollectionExtensions` | `EXTENSION` | **DONE** | 100% | 100% | 100% | **100.00%** | 5 / 0 |
| `XmlArchitecture` | `INFRASTRUCTURE` | **DONE** | 100% | 100% | 100% | **N/A** | Architectural Quality Gate |
| `NativeAOT Runtime Verification` | `INTEGRATION` | **DONE** | 100% | 100% | 100% | **N/A** | Native AOT Smoke Harness |
| **Global Total** | **FRAMEWORK** | **DONE** | **100%** | **100%** | **100%** | **100.00%** | **117 killed / 0 survived** |

---

## Unit Contracts & Behavioral Specifications

### Unit 1: `XmlSchemaCache`
- **Name:** `XmlSchemaCache`
- **Type:** `COMPONENT / PUBLIC_API`
- **Contract:** Implements `IXmlSchemaCache`
- **Inputs:** XSD string, XSD Stream, UTF-8 Span (`ReadOnlySpan<byte>`), file path, directory path.
- **Outputs:** Schema count (`Count`), presence predicates (`ContainsSchema`), global element inspection (`IsRootElementDeclared`).
- **Behavior:** Compiles isolated XSD schemas without resolving external URIs or external entities, storing them thread-safely in precompiled cache entries.
- **Decisions:** Overwrite schemas for the same `targetNamespace` instead of duplicating; encapsulate compilation syntax errors in `XmlSchemaException`.
- **Invariants:**
  - `XmlResolver` is strictly `null` on both `XmlSchemaSet` and internal reader settings.
  - `DtdProcessing` is strictly `Prohibit`.
  - Registration operations do not close caller-supplied streams (`leaveOpen: true`).
- **Error Conditions:**
  - `ArgumentNullException` on null inputs.
  - `FileNotFoundException` when schema file does not exist.
  - `DirectoryNotFoundException` when schema directory does not exist.
  - `XmlSchemaException` on syntactically or semantically invalid schemas.
- **Edge Cases:** Concurrent multi-threaded registrations, schemas with malicious external imports, empty spans, empty directories, concurrent `Clear()` during lookups.
- **Test Coverage:** 48 dedicated unit tests in `XmlSchemaCacheTests.cs`.

---

### Unit 2: `XmlSchemaValidator`
- **Name:** `XmlSchemaValidator`
- **Type:** `COMPONENT / PUBLIC_API`
- **Contract:** Implements `IXmlSchemaValidator`
- **Inputs:** XML string, Stream, `ReadOnlySpan<byte>`, target XSD namespace, `CancellationToken` (async).
- **Outputs:** Functional `Result<bool>` with structured error descriptors (`Error.NotFound`, `Error.Validation`).
- **Behavior:** Validates XML documents against precompiled schemas. Never throws exceptions for malformed XML or validation failures; accumulates errors and warnings according to `XmlValidationOptions` and returns an informative `Error`.
- **Decisions:** Prohibit DTDs unconditionally; retain consumer streams open (`leaveOpen: true`); support configurable warning escalation.
- **Invariants:**
  - Missing schema returns `Error.NotFound` with code `XmlValidation.SchemaNotRegistered`.
  - Schema violation returns `Error.Validation` with code `XmlValidation.SchemaViolation`.
  - Malformed XML or XXE payload returns `Error.Validation` with code `XmlValidation.XmlMalformed`.
  - Streams remain open and readable after validation.
- **Edge Cases:** Empty streams, Billion Laughs XXE payloads, unclosed XML tags, slow async streams with cooperative cancellation.
- **Test Coverage:** 68 unit and integration tests across `XmlSchemaValidatorTests.cs` and `XmlSchemaValidatorAsyncTests.cs`.

---

### Unit 3: `XmlValidationOptions`
- **Name:** `XmlValidationOptions`
- **Type:** `COMPONENT / PUBLIC_API`
- **Contract:** Configuration POCO with secure defaults.
- **Outputs:** Configuration state `IncludeWarnings` (default: `false`), `TreatWarningsAsErrors` (default: `false`).
- **Invariants:** Throws zero exceptions; thread-safe for concurrent read operations.
- **Test Coverage:** Verified in DI and validator option test suites (`XmlValidationDiTests.cs`).

---

### Unit 4: `XmlValidationServiceCollectionExtensions`
- **Name:** `XmlValidationServiceCollectionExtensions`
- **Type:** `EXTENSION / PUBLIC_API`
- **Contract:** Extension methods on `IServiceCollection`: `AddXmlValidation(this IServiceCollection)` and `AddXmlValidation(this IServiceCollection, Action<XmlValidationOptions>?)`.
- **Behavior:** Registers `IXmlSchemaCache` and `IXmlSchemaValidator` as singletons with options configuration and fallback to `NullLogger` if logging is unconfigured.
- **Invariants:** Immediately validates `ArgumentNullException` if `services` is `null`.
- **Test Coverage:** 16 tests in `XmlValidationDiTests.cs`.

---

### Unit 5: `XmlArchitecture`
- **Name:** `XmlArchitecture`
- **Type:** `INFRASTRUCTURE / QUALITY_GATE`
- **Contract:** Automated architectural verification rules via NetArchTest.
- **Behavior:** Verifies zero prohibited third-party dependencies (EF Core, Dapper, Newtonsoft.Json, RabbitMQ, Kafka, AspNetCore, Emit), all interfaces start with 'I', all concrete services are sealed, all public types belong to the root namespace, and zero `[Obsolete]` members exist across types and methods.
- **Test Coverage:** Architecture test suite in `XmlArchitectureTests.cs`.

---

### Unit 6: `NativeAOT Runtime Verification`
- **Name:** `NativeAOT Runtime Verification`
- **Type:** `INTEGRATION / RUNTIME`
- **Contract:** Self-contained native binary compiled with `PublishAot=true`.
- **Behavior:** Verifies that all validation modes, caches, XXE protections, and span parsers execute with zero runtime reflection issues or trimming warnings.
- **Test Coverage:** 20 verified native runtime assertions in `EricksonLopez.Xml.Validation.AotTest`.

---

## Code Coverage Verification

### Multi-Target Coverage Summary (Coverlet with `coverlet.runsettings`)

- **Target Frameworks:** `net8.0`, `net9.0`, `net10.0`
- **Line Coverage:** 100.00% (260 / 260 lines covered)
- **Branch Coverage:** 100.00% (46 / 46 branches covered)
- **Method Coverage:** 100.00% (24 / 24 methods covered)

```xml
<coverage line-rate="1" branch-rate="1" lines-covered="260" lines-valid="260" branches-covered="46" branches-valid="46">
  <package name="EricksonLopez.Xml.Validation" line-rate="1" branch-rate="1" complexity="61">
    <class name="EricksonLopez.Xml.Validation.XmlSchemaCache" line-rate="1" branch-rate="1" />
    <class name="EricksonLopez.Xml.Validation.XmlSchemaValidator" line-rate="1" branch-rate="1" />
    <class name="EricksonLopez.Xml.Validation.XmlSchemaValidator/<ValidateAsync>d__8" line-rate="1" branch-rate="1" />
    <class name="EricksonLopez.Xml.Validation.XmlValidationServiceCollectionExtensions" line-rate="1" branch-rate="1" />
  </package>
</coverage>
```

---

## Stryker.NET Mutation Testing Verification

### Configuration (`stryker-config.json`)

```json
{
  "$schema": "https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/StrykerConfig.schema.json",
  "stryker-config": {
    "project": "src/EricksonLopez.Xml.Validation/EricksonLopez.Xml.Validation.csproj",
    "test-projects": [
      "tests/EricksonLopez.Xml.Validation.Tests/EricksonLopez.Xml.Validation.Tests.csproj"
    ],
    "target-framework": "net10.0",
    "concurrency": 4,
    "mutate": [
      "**/*.cs",
      "!bin/**",
      "!obj/**",
      "!**/*.g.cs",
      "!**/*.AssemblyInfo.cs"
    ],
    "thresholds": {
      "high": 100,
      "low": 98,
      "break": 95
    },
    "ignore-methods": [
      "ConfigureAwait",
      "Dispose"
    ],
    "ignore-mutations": [],
    "reporters": [
      "html",
      "json",
      "progress",
      "cleartext"
    ]
  }
}
```

### Verified Mutation Score

```text
╭─────────────────────────────────────────┬────┬─────┬──────┬───────┬─────┬────╮
│ File                                    │ %  │  #  │   #  │    #  │  #  │ #  │
│                                         │ s… │ ki… │ tim… │ surv… │ no  │ e… │
│                                         │    │     │      │       │ cov │    │
├─────────────────────────────────────────┼────┼─────┼──────┼───────┼─────┼────┤
│ src                                     │ 100│ 117 │    0 │     0 │   0 │  7 │
│ XmlSchemaCache.cs                       │ 100│  42 │    0 │     0 │   0 │  1 │
│ XmlSchemaValidator.cs                   │ 100│  70 │    0 │     0 │   0 │  6 │
│ XmlValidationServiceCollectionExtensions│ 100│   5 │    0 │     0 │   0 │  0 │
╰─────────────────────────────────────────┴────┴─────┴──────┴───────┴─────┴────╯
Total Mutants Tested: 117
Killed: 117
Survived: 0
Timeout: 0
Compile Errors: 7 (excluded from mutation score denominator)
Ignored: 41
Final Mutation Score: 100.00 %
```

### Resolved Mutation Edge Cases

- **Mutant 3:** `XmlSchemaCache.cs` (Line 34): `new XmlSchemaSet { XmlResolver = null }` mutated to `new XmlSchemaSet {}` in `RegisterSchema(string, string)`.
  - **Resolution:** Verified resolver state using `XmlSchemaTestHelper.GetXmlResolver()`, asserting it is strictly `null`. **STATUS: KILLED**.
- **Mutant 9:** `XmlSchemaCache.cs` (Line 49): `new XmlSchemaSet { XmlResolver = null }` mutated to `new XmlSchemaSet {}` in `RegisterSchema(string, Stream)`.
  - **Resolution:** Verified with `schemaSet.GetXmlResolver().Should().BeNull()`. **STATUS: KILLED**.
- **Mutant 18:** `XmlSchemaCache.cs` (Line 68): `new XmlSchemaSet { XmlResolver = null }` mutated to `new XmlSchemaSet {}` in `RegisterSchema(string, ReadOnlySpan<byte>)`.
  - **Resolution:** Verified with `schemaSet.GetXmlResolver().Should().BeNull()`. **STATUS: KILLED**.
- **Mutant 46:** `XmlSchemaCache.cs` (Line 121): `new XmlSchemaSet { XmlResolver = null }` mutated to `new XmlSchemaSet {}` in `RegisterSchemasFromDirectory`.
  - **Resolution:** Verified with `dirSet.GetXmlResolver().Should().BeNull()`. **STATUS: KILLED**.

---

## Source Generators & Observability

- **Consumed Generators:** `Microsoft.Extensions.Logging.Generators.LoggerMessageGenerator` from the .NET Base Class Library to produce compile-time, zero-allocation structured logs (`[LoggerMessage]`, EventIds 1001–1005).
- **Behavioral Verification:** Verified via `TestLogger<T>` interceptor testing log levels, event IDs, and formatted message arguments.

---

## Analyzers & Quality Gates

- **Enabled Analyzers:**
  - `Microsoft.CodeAnalysis.NetAnalyzers` (AnalysisLevel: `latest-recommended`).
  - Native AOT & Trim Analyzers (`EnableTrimAnalyzer: true`, `IsAotCompatible: true`).
  - Roslyn Code Style Enforcement (`EnforceCodeStyleInBuild: true`).
  - `.editorconfig` rules enforcing `error` on `CS0618`, `CS0619`, `CS0159`, and `warning` on `CA1707`, `CA1852`, `CA1305`, `xUnit1051`, `CS1591`.
- **Zero Warnings Policy:** `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- **Result:** 0 warnings, 0 errors across the entire solution.

---

## Architectural Decision Records (ADRs)

- **ADR-TEST-01:** Anti-XXE Resolver Verification — Mandate explicit assertions confirming `XmlResolver = null` across all registration and compilation pathways in `XmlSchemaCache`.
- **ADR-TEST-02:** Coverlet Configuration Isolation — Isolate code coverage measurements strictly to productive code and exclude compiler-generated artifacts.
- **ADR-TEST-03:** Test Harness Self-Verification — Include a control metatest verifying that unconfigured schema sets return non-null resolvers while securely configured ones return `null`.

---

## Verified Execution Evidence

```powershell
# Multi-target test execution
dotnet clean
dotnet restore
dotnet build EricksonLopez.Xml.slnx --configuration Release -p:TreatWarningsAsErrors=true
dotnet test EricksonLopez.Xml.slnx --configuration Release
```

- **Output:**
  - `EricksonLopez.Xml.Validation.Tests.dll (net8.0)`: 193 Passed, 0 Failed.
  - `EricksonLopez.Xml.Validation.Tests.dll (net9.0)`: 193 Passed, 0 Failed.
  - `EricksonLopez.Xml.Validation.Tests.dll (net10.0)`: 193 Passed, 0 Failed.
  - `EricksonLopez.Xml.Validation.ArchitectureTests.dll (net8.0)`: 6 Passed, 0 Failed.
  - `EricksonLopez.Xml.Validation.ArchitectureTests.dll (net9.0)`: 6 Passed, 0 Failed.
  - `EricksonLopez.Xml.Validation.ArchitectureTests.dll (net10.0)`: 6 Passed, 0 Failed.
  - **Total:** 597 tests executed and passed (199 unique tests across 3 target frameworks).

---

## Acceptance Status

```text
[X] All unit and integration contracts identified and tested.
[X] 100% Line Coverage (260/260 lines).
[X] 100% Branch Coverage (46/46 branches).
[X] 100% Method Coverage (24/24 methods).
[X] 100% Mutation Score (117/117 killed, 0 survived).
[X] Zero [Obsolete] APIs across all types and members.
[X] 100% Technical English documentation.
[X] Kebab-case document naming policy enforced.
[X] Clean build under TreatWarningsAsErrors=true.

Status = COMPLIANT
```
