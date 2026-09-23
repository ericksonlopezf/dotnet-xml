---
name: Feature Request
about: Propose a new feature, schema provider, or architectural capability in EricksonLopez.Xml
title: "[FEATURE]: "
labels: ["enhancement"]
assignees: ""
---

### Problem Statement

Is your feature request related to a problem or limitation? Please describe clearly (e.g. *"I need a streaming validator that reports validation events iteratively..."*).

### Proposed Solution

A clear and concise description of what you want to happen. What does the proposed API look like?

```csharp
// Proposed C# API signature or usage pattern
```

### Security & Anti-XXE Impact

All XML parsing must strictly preserve `DtdProcessing.Prohibit` and `XmlResolver = null`.
- How does the proposed feature maintain the strict Anti-XXE invariant?

### Native AOT & Allocation Impact

- Will this capability require reflection or runtime code generation?
- How will this feature maintain zero intermediate allocations or span-based throughput invariants?

### Alternatives Considered

A clear and concise description of any alternative solutions or features you've considered.

### Additional Context

Add any other context or schemas about the feature request here.
