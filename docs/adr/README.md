# Architectural Decision Records (ADR) Index

This directory documents the foundational architectural, security, and performance decisions governing `EricksonLopez.Xml.Validation`.

| ADR | Title | Status | Date |
|:---:|:------|:------:|:----:|
| [ADR-001](adr-001-anti-xxe-by-default.md) | Anti-XXE Enforced by Default | Accepted | 2026-09-02 |
| [ADR-002](adr-002-result-instead-of-exceptions.md) | Result Pattern Instead of Exceptions for Schema Violations | Accepted | 2026-09-02 |
| [ADR-003](adr-003-namespace-as-cache-key.md) | Target Namespace as the Primary Cache Key | Accepted | 2026-09-02 |
| [ADR-004](adr-004-warnings-configurable.md) | Configurable Schema Warning Handling | Accepted | 2026-09-02 |
| [ADR-005](adr-005-no-schematron.md) | Omission of Schematron and RelaxNG Support | Accepted | 2026-09-02 |
| [ADR-006](adr-006-mutation-testing-quality-gate.md) | Mutation Testing Quality Gate and Release Verification | Accepted | 2026-09-02 |
| [ADR-007](adr-007-zero-allocation-span-validation.md) | Reduced-Allocation ReadOnlySpan Validation Modality | Accepted | 2026-09-02 |
| [ADR-008](adr-008-async-stream-cancellation-tokens.md) | Asynchronous Streaming Validation with Cooperative Cancellation | Accepted | 2026-09-02 |
| [ADR-009](adr-009-logger-message-partial-methods.md) | Zero-Allocation Diagnostic Logging via LoggerMessage Attribute | Accepted | 2026-09-02 |
| [ADR-010](adr-010-service-collection-extensions-lifetime.md) | Singleton Service Lifetime for Schema Cache and Validator | Accepted | 2026-09-02 |
| [ADR-011](adr-011-native-aot-compilation-pipeline.md) | Native AOT Compilation Pipeline and Smoke Verification | Accepted | 2026-09-02 |
| [ADR-012](adr-012-error-taxonomy-code-structure.md) | Structured Error Taxonomy and Diagnostic Error Codes | Accepted | 2026-09-02 |
| [ADR-013](adr-013-thread-safe-precompiled-schema-cache.md) | Thread-Safe Precompiled Schema Cache Architecture | Accepted | 2026-09-02 |
| [ADR-014](adr-014-benchmark-regression-policy.md) | Continuous Benchmark Regression Policy | Accepted | 2026-09-02 |
| [ADR-015](adr-015-multi-targeting-strategy.md) | Multi-Targeting Strategy (.NET 8.0, 9.0, 10.0) | Accepted | 2026-09-02 |
| [ADR-016](adr-016-architecture-testing-and-ci-cd-quality-gates.md) | Architecture Testing Suite and CI/CD Quality Gate Standardization | Accepted | 2026-09-03 |
| [ADR-017](adr-017-extensibility-boundary-and-schema-cache-contract-segregation.md) | Extensibility Boundary and Schema Cache Contract Segregation | Accepted | 2026-09-13 |
