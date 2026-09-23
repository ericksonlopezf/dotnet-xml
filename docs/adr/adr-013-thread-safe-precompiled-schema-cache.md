# ADR-013: Thread-Safe Precompiled Schema Cache Architecture

- **Status:** Accepted (Amended)
- **Date:** 2026-09-02 (Amended: 2026-09-13)
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

In multi-threaded server environments, multiple threads validate documents concurrently against the same schemas, while background workers or dynamic configuration updates may register new schemas concurrently.

The BCL `XmlSchemaSet` is not thread-safe for simultaneous mutation and query operations. Furthermore, exposing mutable `XmlSchemaSet` references externally would permit callers to re-attach unsafe resolvers or alter compiled schemas after insertion (XML-API-001).

## Decision

`XmlSchemaCache` encapsulates a `ConcurrentDictionary<string, SchemaCacheEntry>` using `StringComparer.Ordinal`:
- Each registered schema set is compiled completely in isolation before insertion into the dictionary.
- Encapsulates `SchemaCacheEntry` containing both the compiled `XmlSchemaSet` and a precomputed `HashSet<(string LocalName, string NamespaceUri)>` of global root elements.
- Access to the underlying `XmlSchemaSet` is strictly segregated to the internal interface `IXmlSchemaSetProvider`, preventing external mutation.
- The public interface `IXmlSchemaCache` exposes only safe inspection methods (`ContainsSchema`, `Count`, `IsRootElementDeclared`) and registration overloads.
- Prohibits external entity resolution during compilation (`schemaSet.XmlResolver = null`).

## Consequences

### Positive
- Lock-free, wait-free read paths during document validation.
- Atomic registration prevents partial or corrupt schema sets from being exposed to concurrent readers.
- Predictable lookup performance under massive parallelism.

### Negative
- Schema compilation is executed prior to dictionary publication; registering massive schemas may briefly consume CPU on the registration thread.
