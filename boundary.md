# Architectural Boundary Specification: EricksonLopez.Xml.Validation

## 1. Purpose

`EricksonLopez.Xml.Validation` provides enterprise-grade, memory-efficient, Anti-XXE-enforced XSD schema validation and precompiled schema caching for modern .NET applications.

---

## 2. Invariants & Guardrails

1. **Anti-XXE by Default**: Inline DTD processing (`DtdProcessing.Prohibit`) and external entity resolution (`XmlResolver = null`) are mandatory across all execution paths and cannot be disabled via public API.
2. **Zero Exceptions for Schema Violations**: All validation, syntax, and schema errors are mapped functionally to `Result<bool>` with structured error descriptors (`Error.Validation`, `Error.NotFound`, etc.).
3. **Reduced Heap Allocation**: Direct validation of `ReadOnlySpan<byte>` eliminates intermediate string allocations when parsing XML from network buffers or UTF-8 byte streams.
4. **Native AOT & Trimming Safety**: Built with `IsAotCompatible=true` and `EnableTrimAnalyzer=true`. Zero reflection over arbitrary types.
5. **Thread Safety**: The precompiled schema cache (`XmlSchemaCache`) is internally backed by a thread-safe `ConcurrentDictionary` and supports concurrent read/write operations without external locking.

---

## 3. Explicit Boundary Exclusions

- **No Schematron / RelaxNG Support**: Confined strictly to W3C XML Schema Definition (XSD) standards (see ADR-005).
- **No In-Place DOM Mutation**: Focuses purely on schema validation and integrity verification, not document generation or XPath transformation.
- **No Legacy Serialization**: Does not replace or depend on `XmlSerializer` or `DataContractSerializer`.
