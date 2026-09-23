# Best Practices — EricksonLopez.Xml.Validation

Engineering, performance, security, and observability best practices for architectures leveraging `EricksonLopez.Xml.Validation`.

---

## 1. Service Lifecycle Management

- **Always Register as Singletons:** Both `IXmlSchemaCache` and `IXmlSchemaValidator` are designed and verified as thread-safe singleton services. They have no per-request mutable state.
- **Preload Schemas at Application Startup:** Compile all required schemas during host initialization (e.g., via `IHostedService` or startup configuration blocks). Avoid calling `RegisterSchema` dynamically inside hot HTTP request endpoints.

---

## 2. Performance and Memory Optimization

- **Use `ReadOnlySpan<byte>` for In-Memory Buffers:** If the XML document is already loaded in memory or retrieved from a buffer pool (`ArrayPool<byte>`), call `Validate(ReadOnlySpan<byte>, targetNamespace)` to minimize intermediate `string` heap allocations.
- **Use `ValidateAsync(Stream)` for Large Network Payloads:** When processing incoming HTTP request bodies or message broker payloads, stream the data directly to the validator to keep memory utilization flat.
- **Rely on Stream Preservation:** The synchronous stream overload honors `leaveOpen: true`, allowing callers to rewind and reuse the stream for downstream processing after validation succeeds.

---

## 3. Defensive Security Posture

- **Enforce Anti-XXE as a Strict Invariant:** Never attempt to circumvent Anti-XXE protections. All incoming XML content must be processed with DTD processing prohibited and external entity resolution disabled.
- **Validate Before Deserializing:** Never invoke an XML deserializer (`XmlSerializer`, `DataContractSerializer`, `XDocument.Parse`) on untrusted data without first passing the raw payload through `IXmlSchemaValidator`. The validator acts as a defensive perimeter firewall.

---

## 4. Observability and Diagnostics

- **Configure `Microsoft.Extensions.Logging`:** The validator emits detailed diagnostic events under `LogLevel.Debug` using compile-time source-generated `[LoggerMessage]` partial methods, guaranteeing zero heap allocations on high-throughput paths.
- **Monitor Validation Anomalies:** Aggregate Event IDs `1002` (Validation Failed) and `1005` (Malformed XML / DTD Injection) in your SIEM or APM dashboards to proactively detect potential penetration attempts or malformed upstream clients.
