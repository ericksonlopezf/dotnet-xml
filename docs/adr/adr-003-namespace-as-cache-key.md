# ADR-003: targetNamespace as the 1:1 Cache Key

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

`XmlSchemaCache` must map incoming XML documents to their corresponding compiled `XmlSchemaSet`. A keying strategy must be chosen.

Options considered:

1. **targetNamespace (one `XmlSchemaSet` per namespace):** Each namespace maps to exactly one compiled schema set.
2. **File path:** Schemas are keyed by source file. A namespace can have multiple files.
3. **Composite key (namespace + schema name):** More granular, but requires callers to provide two identifiers per validation call.
4. **Auto-detect from XML:** Parse the XML to find its namespace, then look up. Implicit.

## Decision

`IXmlSchemaCache` uses `targetNamespace` (a `string`) as the sole cache key:

```csharp
// Public registration API (IXmlSchemaCache)
void RegisterSchema(string targetNamespace, string xsdContent);
bool ContainsSchema(string targetNamespace);

// Internal schema retrieval (IXmlSchemaSetProvider — internal interface, not public)
// bool TryGetSchemaSet(string targetNamespace, out XmlSchemaSet? schemaSet);
// ^ Moved to IXmlSchemaSetProvider (XML-API-001, see ADR-017) to prevent
//   external callers from acquiring mutable XmlSchemaSet references.
```

> [!NOTE]
> **Post-refactoring (XML-API-001 / ADR-017):** `TryGetSchemaSet` was removed from the public `IXmlSchemaCache` interface and moved to the `internal interface IXmlSchemaSetProvider`. The public `IXmlSchemaCache` exposes only safe, immutable operations (`RegisterSchema`, `ContainsSchema`, `Count`, `IsRootElementDeclared`, `Clear`). Refer to [ADR-017](adr-017-extensibility-boundary-and-schema-cache-contract-segregation.md) for full rationale.

Callers provide the `targetNamespace` at validation time:

```csharp
validator.Validate(xml, "https://example.com/orders");
```

## Consequences

### Positive

- **Simple, predictable API:** One string → one schema set. No ambiguity.
- **Explicit namespace coupling:** Callers must know which namespace their XML belongs to. This is already required for correct XSD validation.
- **Performance:** `ConcurrentDictionary<string, SchemaCacheEntry>` lookup is O(1) with string hash (`StringComparer.Ordinal`). Each `SchemaCacheEntry` holds the compiled `XmlSchemaSet` plus a pre-indexed `HashSet` of global elements for `IsRootElementDeclared`.
- **Thread-safe by construction:** `ConcurrentDictionary` provides lock-free reads and atomic insertions.

### Negative

- **1:1 constraint:** If a logical namespace is split across multiple XSD files (using `xs:include`), only one `XmlSchemaSet` can be registered per namespace. The second registration overwrites the first.
  - **Mitigation:** For `xs:include` patterns, merge included files into a single XSD, or register only the root XSD that `xs:include`s the others (if file resolution is available).
- **No auto-detection:** Callers cannot omit the namespace and have it inferred from the XML. Explicit is preferred over implicit (POLA).
- **`xs:import` with `XmlResolver = null`:** Imported schemas with remote URIs are ignored (see ADR-001). This can produce silent validation gaps if schemas are not pre-registered.

### Rejected alternatives

- **File-path keying:** Rejected. Two files with the same namespace create lookup ambiguity. File paths are implementation details — namespaces are the semantic contract.
- **Auto-detect from XML:** Rejected. Requires pre-parsing the XML to extract the namespace before validation. Adds latency and complexity. Also ambiguous for documents without a default namespace.
- **Multiple `XmlSchemaSet` per namespace:** Rejected. Increases lookup complexity and is rarely needed in practice (most enterprises have one canonical schema per namespace). Where multiple XSD files share a namespace, `RegisterSchemasFromDirectory` merges them into a single `XmlSchemaSet` before insertion.
