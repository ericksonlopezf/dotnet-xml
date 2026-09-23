# ADR-014: Continuous Benchmark Regression Policy

- **Status:** Accepted (Amended)
- **Date:** 2026-09-02 (Amended: 2026-09-13)
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

Performance degradation in foundational libraries cascades into all downstream consumer services. Without automated regression gates in CI, subtle performance regressions (such as accidental string allocations or unnecessary LINQ enumerations) easily escape into production releases.

## Decision

1. We establish a dedicated micro-benchmarks suite (`benchmarks/EricksonLopez.Xml.Validation.Benchmarks`) utilizing BenchmarkDotNet.
2. A CI gate (`benchmark-regression-gate.yml`) runs on PRs touching `src/**` or `benchmarks/**` and compares execution means against the baseline on `main` via `scripts/verify-benchmark-gate.ps1`.
3. If any benchmark exceeds the **5% latency regression threshold** or violates the **0 B allocation invariant** for in-memory cache lookups, the CI gate FAILS immediately.
4. Deep multi-framework weekly benchmarks run on schedule across .NET 8, 9, and 10.

## Consequences

### Positive
- Prevents accidental performance or memory allocation regressions from merging.
- Maintains empirical transparency for library throughput and GC metrics.
- Forces architectural accountability on PR authors.

### Negative
- CI runs on shared cloud runners exhibit minor statistical variance; the 10% threshold balances noise tolerance with regression detection.
