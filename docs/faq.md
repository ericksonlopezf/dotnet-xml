# Frequently Asked Questions (FAQ) — EricksonLopez.Xml.Validation

Technical answers to common architectural, security, and operational questions.

---

## 1. Why does `EricksonLopez.Xml.Validation` unconditionally prohibit DTDs?
Document Type Definitions (DTDs) are the primary vector for **XML External Entity (XXE) attacks**, which allow remote attackers to exfiltrate sensitive local files (`/etc/passwd`, environment variables), perform Server-Side Request Forgery (SSRF), or cause Denial of Service via recursive entity expansion ("Billion Laughs"). In alignment with OWASP guidelines and [ADR-001](adr/adr-001-anti-xxe-by-default.md), `DtdProcessing.Prohibit` and `XmlResolver = null` are permanently enforced and cannot be overridden via configuration.

---

## 2. How should schemas with `<xs:include>` and `<xs:import>` be handled?
- **`<xs:include>` within the same `targetNamespace`:** The library compiles schemas on a per-file basis. If your schema is split across multiple files for the same target namespace, consolidate them into a single file before registration, or register the root XSD that locally includes the auxiliary files.
- **`<xs:import>` across different namespaces:** Download all imported external schemas and register them individually in `IXmlSchemaCache` before registering the root dependent schema.

---

## 3. Is it safe to share a single `IXmlSchemaCache` instance across multiple threads?
**Yes, 100%.** `XmlSchemaCache` uses a `ConcurrentDictionary<string, SchemaCacheEntry>` internally with lock-free reads. Multiple background workers and concurrent requests can safely perform lookups and register schemas simultaneously without race conditions or memory corruption.

---

## 4. Why does the validator return `Result<bool>` instead of throwing exceptions?
Throwing exceptions (`throw`) in .NET incurs substantial overhead, including stack trace generation, unwinding, and GC pressure. Schema validation failures (e.g., missing fields, format violations) are **expected business outcomes**, not exceptional runtime crashes. Returning `Result<bool>` enables high-throughput Railway-Oriented Programming without exception overhead.

---

## 5. Is the library compatible with Native AOT (Ahead-of-Time)?
**Yes.** The package has `<IsAotCompatible>true</IsAotCompatible>` and `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>` enabled. It does not use reflection, `dynamic` dispatch, or runtime code generation. Structured logging uses C# compile-time `[LoggerMessage]` source generators. Compatibility is verified by the `tests/EricksonLopez.Xml.Validation.AotTest` smoke test project.

---

## 6. How does `ValidateAsync` handle cooperative cancellation?
`ValidateAsync` performs a **pre-flight check** of the supplied `CancellationToken` before streaming begins — if the token is already cancelled, `OperationCanceledException` is thrown immediately without reading any data. During streaming, the token is also checked on every node read (`xmlReader.ReadAsync()`), so long-running validations are interrupted promptly. On cancellation, Event ID `1004 (LogValidationCancelled)` is logged and stream handles are released.
