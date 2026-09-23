# ADR-016: Architecture Testing Suite and CI/CD Quality Gate Standardization

- **Status:** Accepted (Amended)
- **Date:** 2026-09-03 (Amended: 2026-09-13)
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

To maintain long-term architectural integrity across the `EricksonLopez` ecosystem, every foundational library must adhere to non-negotiable governance gates:
1. **Architectural Invariants Verification**: Enforcing rules such as sealed concrete classes, interface naming conventions (`I*`), namespace isolation, and zero obsolete types must be automated via executable unit tests rather than manual PR reviews.
2. **CI/CD Quality Gates Parity**: Workflows must support reusable triggers (`workflow_call`), report status checks directly to the GitHub Statuses API, execute SonarCloud deep static analysis, and publish packages via OIDC with Sigstore provenance attestation.

Previously, `dotnet-xml` lacked an independent `ArchitectureTests` project, omitted SonarCloud execution steps in `dotnet-build-test.yml`, and was missing `workflow_call` in `mutation-testing.yml`, which caused failures during automated release publication.

## Decision

1. **Introduce Architecture Testing Project**:
   - Create `EricksonLopez.Xml.Validation.ArchitectureTests` using `NetArchTest.Rules` and `AwesomeAssertions`.
   - Implement tests asserting:
     - Dependency isolation: zero forbidden dependencies on ORMs (`EntityFrameworkCore`, `Dapper`), message brokers (`RabbitMQ`, `Kafka`), or web frameworks (`AspNetCore`).
     - Concrete service sealing: `XmlSchemaValidator` and `XmlSchemaCache` must be sealed.
     - Interface prefixing (`I*`) and namespace adherence (`EricksonLopez.Xml.Validation*`).
     - Zero `[Obsolete]` APIs in production code.
     - Abstract interface contracts: `IsRootElementDeclared` is verified as an abstract method on `IXmlSchemaCache` implemented directly in `XmlSchemaCache`, avoiding Default Interface Methods that cannot inspect encapsulated cache entries.

2. **Standardize CI/CD Pipeline Infrastructure**:
   - Add `workflow_call` to `.github/workflows/mutation-testing.yml` with `full-run` boolean input.
   - Implement automated GitHub Commit Status creation (`mutation-testing/stryker`) so that `verify-mutation-gate.js` reliably inspects test results on `main`.
   - Incorporate SonarCloud static analysis (Java 17 setup, `dotnet-sonarscanner begin/end`) into `dotnet-build-test.yml`.
   - Align `publish.yml` with Tier-1 OIDC trusted publishing, Sigstore provenance attestation, and automatic GitHub Release creation.

## Consequences

### Positive
- Prevents architectural erosion automatically in PR builds.
- Enables seamless release publication via `publish.yml` without workflow dispatch failures.
- Ensures total CI/CD governance consistency with Tier-1 repositories (`dotnet-sql-builder`, `dotnet-processes`, `dotnet-outbox`).

### Negative
- Adds a small incremental build duration in CI to run architecture tests and SonarCloud scanning.
