# Project Governance & Quality Policy: EricksonLopez.Xml

This document formalizes the engineering governance, quality assurance policies, and supply chain security standards enforced across the **`EricksonLopez.Xml`** library ecosystem.

## 1. Quality Gates & Enforcement

`EricksonLopez.Xml` adheres to strict zero-tolerance quality and architecture standards:

- **100% Deterministic Builds**: All builds are reproducible and enforce `TreatWarningsAsErrors=true` with warning level 5.
- **Supply Chain Security**: Packages are signed with the project Strong Name Key (`EricksonLopez.snk`), attested with Sigstore provenance (`actions/attest-build-provenance`), and published via NuGet Trusted Publishing (OIDC).
- **Mutation Testing Gate**: Mandatory **95% break threshold** enforced via Stryker.NET on every release candidate.
- **Performance Regression Gate**: Continuous performance tracking via BenchmarkDotNet; pull requests with mean latency regression > 5% or heap allocations on zero-allocation paths are blocked.
- **Native AOT Validation**: Executed natively on Linux in CI before package deployment.

---

## 2. Release & Versioning Policy

- All commits follow [Conventional Commits](https://www.conventionalcommits.org/).
- Releases are automated via [Release Please](https://github.com/googleapis/release-please).
- Semantic versioning (SemVer 2.0.0) is strictly respected.
