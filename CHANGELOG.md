# Changelog

All notable changes to `EricksonLopez.Xml.Validation` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] — 2026-09-23

### Added
- **Core Schema Cache Engine (`IXmlSchemaCache`, `XmlSchemaCache`, `XmlSchemaCacheExtensions`):**
  - Thread-safe in-memory cache backed by `ConcurrentDictionary<string, SchemaCacheEntry>` with pre-indexed global elements for $O(1)$ root element declaration lookups (`IsRootElementDeclared`).
  - Strict Anti-XXE defense during compilation (`XmlSchemaSet.XmlResolver = null`, `DtdProcessing.Prohibit`).
  - Zero-allocation memory optimization in `RegisterSchema(string, ReadOnlySpan<byte>)` leveraging native memory pointers (`fixed`) and `UnmanagedMemoryStream`.
  - Overloads for registering schemas from `string`, `Stream`, and `ReadOnlySpan<byte>` (UTF-8).
  - High-level extension methods in `XmlSchemaCacheExtensions` for file registration (`RegisterSchemaFile`) and recursive directory scanning (`RegisterSchemasFromDirectory`) with multi-file namespace grouping.
  - Architectural boundary segregation per ADR-017: internal `IXmlSchemaSetProvider` isolates mutable `XmlSchemaSet` instances from external mutation while supplying precompiled schemas to the engine.
  - Cache lookup and query methods: `ContainsSchema`, `Count`, `IsRootElementDeclared`, and `Clear`.
- **Validation Engine (`IXmlSchemaValidator`, `XmlSchemaValidator`):**
  - High-performance XSD validation engine enforcing Anti-XXE (`DtdProcessing.Prohibit`, `XmlResolver = null`) across all code paths.
  - Multi-modal document validation: `string`, `Stream`, `ReadOnlySpan<byte>` (zero intermediate heap allocations via direct `UnmanagedMemoryStream`), and asynchronous streaming (`ValidateAsync` with `CancellationToken`).
  - Strict root element declaration verification (`ValidateRootElement`) before document body streaming.
  - Optimized stream validation path directly executing `XmlReader.Create(Stream)` with `leaveOpen: true` preserving XML encoding declarations.
  - Configurable schema warning event emission via `XmlSchemaValidationFlags.ReportValidationWarnings`.
  - Structured error reporting using `EricksonLopez.Result.Result<bool>` (no exceptions for schema violations, malformed XML, or missing schemas).
  - Fine-grained error metadata including line number and line position from `IXmlLineInfo`.
  - Zero-allocation diagnostic logging using C# compiler `[LoggerMessage]` partial methods (Event IDs 1001–1005).
- **Configuration & Dependency Injection:**
  - `XmlValidationOptions` class supporting `IncludeWarnings`, `TreatWarningsAsErrors`, DoS mitigation controls `MaxCharactersInDocument` (default 10M characters) and `MaxErrors` (default 100 errors), and `ProcessInlineSchema` (default `false`).
  - `IServiceCollection.AddXmlValidation()` extension methods using `TryAddSingleton` for registering `IXmlSchemaCache` and `IXmlSchemaValidator`.
- **Reference Implementation & Showcase (`samples/EricksonLopez.Xml.Showcase`):**
  - 11 progressive executable levels (Levels 00 to 10) demonstrating threat modeling, quick start, DI, enterprise schema patterns (schemas inspired by UBL 2.1 e-Invoice & ISO 20022 `pain.001` — simplified for demonstration purposes), advanced modalities, concurrency, error handling, micro-benchmarks, decorators, queue boundaries, and perimeter defensive pipelines.
  - Matching technical documentation for all showcase levels in `docs/showcase/`.
- **Automated Verification & Quality Gates:**
  - 193 unit and integration tests per target framework (579 total) covering concurrent reads/writes, cancellation, DI, warning reporting, DoS prevention limits, adversarial security attacks, and malformed XML.
  - Dedicated Architecture Testing suite (`tests/EricksonLopez.Xml.Validation.ArchitectureTests`) enforcing naming, sealing, and dependency boundaries via NetArchTest (6 tests per framework, 18 total).
  - Comprehensive Benchmark suite (`benchmarks/EricksonLopez.Xml.Validation.Benchmarks`) evaluating throughput, allocations, and schema cache latency under BenchmarkDotNet.
  - Automated benchmark regression gate script (`scripts/verify-benchmark-gate.ps1`) and GitHub Actions workflow (`.github/workflows/benchmark-regression-gate.yml`) asserting zero heap allocations and $\le 5\%$ latency variance.
  - Automated zero-tolerance repository compliance verification suite (`scripts/verify-compliance.ps1`).
  - Mutation testing quality gate verification scripts and automated Stryker reporting (`scripts/verify-mutation-gate.js`, `scripts/record-stryker-result.js`).
  - Native AOT publish smoke verification project (`tests/EricksonLopez.Xml.Validation.AotTest`).
  - Comprehensive testing and mutation testing roadmap guide `docs/testing-roadmap.md`.
- **Build, Security & Packaging:**
  - Strongly named assembly signed with `EricksonLopez.snk` (`PublicKeyToken=f3a287785b2818a1`).
  - Multi-targeting for `net8.0`, `net9.0`, and `net10.0`.
  - Native AOT compatibility (`IsAotCompatible=true`, `EnableTrimAnalyzer=true`).
  - Deterministic builds and SourceLink integration.
  - Official NuGet package branding icon (`icon.png`) and MSBuild packaging integration (`<PackageIcon>icon.png</PackageIcon>`).
  - Central Package Management (`Directory.Packages.props`).
  - Sigstore Provenance Attestation (`actions/attest-build-provenance`) and NuGet Trusted Publishing via OIDC (`NuGet/login`) configured in `publish.yml`.
- **Architectural Documentation:**
  - Formal Architecture Decision Records (ADR-001 through ADR-017) in `docs/adr/`.
  - Dedicated engineering guide `docs/build-and-quality.md` and package specification `docs/package-ecosystem.md`.
  - Complete architecture, API reference, performance, best practices, cookbook, and troubleshooting guides in `docs/`.

[Unreleased]: https://github.com/ericksonlopezf/dotnet-xml/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/ericksonlopezf/dotnet-xml/releases/tag/v1.0.0
