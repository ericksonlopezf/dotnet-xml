## Description

Briefly describe the motivation and context for this change. Include any relevant issue numbers (e.g. `Fixes #123`).

---

## Type of Change

- [ ] `feat`: New feature or validation capability
- [ ] `fix`: Bug fix or security remediation (e.g., Anti-XXE defense)
- [ ] `perf`: Memory optimization or allocation reduction (e.g., Span/ReadOnlySpan)
- [ ] `refactor`: Structural improvement without behavioral changes
- [ ] `docs`: Documentation, guide, or ADR update
- [ ] `test`: Additional test cases or mutation test coverage
- [ ] `chore`: Build, packaging, or dependency update

---

## Packages Affected

- [ ] `EricksonLopez.Xml.Validation`
- [ ] Non-package files (Docs, Samples, Benchmarks, Workflows)

---

## Quality Gates & Verification Checklist

- [ ] **Build**: `dotnet build EricksonLopez.Xml.slnx -c Release` completes with 0 errors and 0 warnings (`TreatWarningsAsErrors` enabled).
- [ ] **Automated Tests**: `dotnet test EricksonLopez.Xml.slnx -c Release` passes across .NET 8.0, 9.0, and 10.0.
- [ ] **Architecture Rules**: NetArchTest suite passes (`EricksonLopez.Xml.Validation.ArchitectureTests`).
- [ ] **Native AOT & Trimming**: No trimming warnings; AOT smoke test publishes cleanly (`tests/EricksonLopez.Xml.Validation.AotTest`).
- [ ] **Security & Anti-XXE**: `DtdProcessing.Prohibit` and `XmlResolver = null` maintained across all code paths.
- [ ] **Error Handling**: Uses `Result<T>` from `EricksonLopez.Result` (zero exceptions for validation control flow).
- [ ] **Mutation Testing**: Meets the Tier-1 Stryker break threshold (≥95%).
- [ ] **Documentation**: `CHANGELOG.md` updated and relevant `/docs/` guides modified or added.
- [ ] **Commits**: Follows [Conventional Commits](https://www.conventionalcommits.org/).
