# ADR-011: Native AOT Compilation Pipeline and Smoke Verification

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

Cloud-native microservices and serverless functions demand instantaneous startup times and minimal memory footprints via Ahead-of-Time (Native AOT) compilation.

Many XML libraries in the .NET ecosystem fail under Native AOT due to dynamic code generation, undocumented reflection, and unsupported trimming.

## Decision

1. The library enables `<IsAotCompatible>true</IsAotCompatible>` and `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>` across all target builds.
2. An autonomous smoke verification application (`tests/EricksonLopez.Xml.Validation.AotTest`) is published in CI as a true self-contained native binary (`PublishAot=true`) targeting both `-r linux-x64` and `-r win-x64`.
3. The resulting native binary is executed directly in CI with assertion checks to prove trimming safety.

## Consequences

### Positive
- Guarantees true Ahead-of-Time execution without runtime reflection surprises.
- Provides immediate CI failure if unsupported reflection or dynamic APIs are introduced.
- Enables deployment into containerized environments with sub-10ms startup.

### Negative
- Dynamic schema generation or runtime code emission cannot be added to the engine.
