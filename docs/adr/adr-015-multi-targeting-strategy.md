# ADR-015: Multi-Targeting Strategy (.NET 8.0, 9.0, 10.0)

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

Enterprise consumers adopt .NET Long Term Support (LTS) and Standard Term Support (STS) versions at differing cadences. Restricting the library exclusively to .NET 10 prevents adoption by enterprise teams still standardized on .NET 8 LTS or .NET 9.

## Decision

The core library multi-targets:
```xml
<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
```
- All language features leverage C# latest (`<LangVersion>latest</LangVersion>`).
- Package validation (`<EnablePackageValidation>true</EnablePackageValidation>`) ensures API surface symmetry across all target frameworks.
- Test suites run across all target frameworks in CI.

## Consequences

### Positive
- Broad ecosystem compatibility across active .NET versions.
- Ensures consumers on .NET 8 LTS benefit from modern memory safety and Anti-XXE protections without upgrading the entire host runtime.

### Negative
- CI matrix build and test times increase proportionally to the number of target frameworks.
