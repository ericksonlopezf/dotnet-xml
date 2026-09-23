# ADR-006: Stryker.NET Mutation Testing Quality Gate and Release Verification

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

Code coverage alone is an insufficient indicator of test quality: high line or branch coverage can be achieved with weak or missing assertions. In security-critical libraries providing Anti-XXE defense and schema validation, testing sensitivity must be verified by introducing synthetic code mutations (mutants) and ensuring that the test suite detects and kills them.

In earlier configurations, the break threshold was set to 75%, allowing up to 25% of surviving mutants. Furthermore, the release pipeline (`publish.yml`) did not enforce that mutation testing had successfully passed prior to package publication.

## Decision

1. **Uncompromised Tier-1 Quality Thresholds:**
   Stryker.NET configuration ([`stryker-config.json`](../../stryker-config.json)) enforces:
   - `high: 100`
   - `low: 98`
   - `break: 95` (Stryker exits with non-zero code if score is < 95%).

2. **Automated Mutation Pipeline:**
   A dedicated GitHub Actions workflow ([`.github/workflows/mutation-testing.yml`](../../.github/workflows/mutation-testing.yml)) executes mutation analysis on scheduled intervals and pull requests.

3. **Release Pipeline Gating:**
   Package publication to NuGet.org is gated on passing the Stryker mutation quality gate on the target release commit.

## Consequences

### Positive
- Guarantees parity with Tier-1 ecosystem packages (`dotnet-sql-builder` and `dotnet-outbox`).
- Eliminates silent test regression and ensures assertion rigor across all validation code paths.
- Provides cryptographic and architectural confidence that edge-case mutations cannot slip into production releases.

### Negative
- Mutation testing execution requires higher CI compute time than plain unit test runs.
- Legitimate equivalent mutants must be explicitly suppressed via `ignore-methods` or inline annotations with technical rationale.
