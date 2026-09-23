# ADR-017: Extensibility Boundary and Schema Cache Contract Segregation

- **Status:** Accepted
- **Date:** 2026-09-13
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

The core validation engine (`XmlSchemaValidator`) requires access to compiled `XmlSchemaSet` instances to configure the underlying BCL `XmlReaderSettings.Schemas` on every validation pipeline invocation.

However, the BCL `XmlSchemaSet` is a mutable, stateful class:
1. It exposes a public setter for `XmlResolver`. If external consumers acquire direct references to compiled `XmlSchemaSet` instances, they could attach external resolvers, completely bypassing Anti-XXE protections.
2. Mutation of an `XmlSchemaSet` while other threads are validating documents against it causes undefined behavior and race conditions in concurrent server environments.

In earlier designs, `IXmlSchemaCache` exposed `TryGetSchemaSet(string, out XmlSchemaSet?)`. Architectural refactor XML-API-001 removed this method from the public interface to protect cache immutability and Anti-XXE invariants.

## Decision

We establish an explicit architectural segregation between public cache management and engine-level schema retrieval:

1. **Public Interface (`IXmlSchemaCache`):**
   - Exposes only safe, immutable query and registration contracts: `RegisterSchema(...)`, `ContainsSchema(...)`, `Count`, `IsRootElementDeclared(...)`, and `Clear()`.
   - Never exposes mutable `XmlSchemaSet` handles to external callers.

2. **Internal Provider Interface (`IXmlSchemaSetProvider`):**
   - Declared as `internal interface IXmlSchemaSetProvider`.
   - Implemented by `XmlSchemaCache` to provide internal $O(1)$ access to precompiled `XmlSchemaSet` instances.

3. **Validator Engine Contract Requirement:**
   - `XmlSchemaValidator` accepts `IXmlSchemaCache` in its public constructors for standard dependency injection.
   - At execution time, `XmlSchemaValidator` requires the cache instance to implement `IXmlSchemaSetProvider`.
   - If an external consumer supplies an incompatible custom `IXmlSchemaCache` implementation that does not implement `IXmlSchemaSetProvider`, validation methods throw a descriptive `InvalidOperationException`:
     `"The provided IXmlSchemaCache instance (...) must implement the internal IXmlSchemaSetProvider interface to be used by the validator."`

4. **Extensibility Guidance:**
   - Consumers wishing to augment caching (e.g., telemetry, logging, metrics) should decorate `IXmlSchemaValidator` or wrap `IXmlSchemaCache` while preserving the standard cache engine, as demonstrated in Showcase Level 08.

## Consequences

### Positive
- **Guaranteed Immutability:** Precompiled schemas in the cache cannot be mutated by consumer code once registered.
- **Strict Anti-XXE Preservation:** External entity resolution remains permanently disabled without possibility of tampering.
- **Explicit Failure Mode:** Clear, actionable exception messages if an unsupported cache mock or stub is provided.

### Negative
- Custom third-party implementations of `IXmlSchemaCache` cannot be passed directly to `XmlSchemaValidator` without implementing the internal provider interface.
