# ADR-001: Anti-XXE Enforced by Default

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

XML External Entity (XXE) injection is listed in the [OWASP Top 10](https://owasp.org/www-project-top-ten/) and has caused critical security vulnerabilities in production systems that process XML from external sources.

The .NET BCL (`System.Xml.Schema`) is **opt-in secure**: developers must explicitly set `DtdProcessing.Prohibit` and `XmlResolver = null` on every `XmlReaderSettings` instance. Failing to do so leaves applications vulnerable by default.

## Decision

`EricksonLopez.Xml.Validation` enforces Anti-XXE on **every validation code path** without exception:

```csharp
// Applied unconditionally in XmlSchemaValidator.CreateValidationSettings()
new XmlReaderSettings
{
    DtdProcessing = DtdProcessing.Prohibit,  // Prohibits <!DOCTYPE ...>
    XmlResolver   = null,                     // Disables external entity resolution
    // ...
};

// Applied unconditionally in XmlSchemaCache when compiling schemas
schemaSet.XmlResolver = null;
```

The Anti-XXE configuration is **not exposed in the public API**. There is no option to disable it.

## Consequences

### Positive

- Applications that use this library are protected from XXE by default, with zero developer effort.
- Security review is simplified: the Anti-XXE guarantee is architectural, not configuration-dependent.
- Compliant with OWASP XML Security Cheat Sheet out of the box.

### Negative

- XML documents with `<!DOCTYPE ...>` declarations will fail validation with a `XmlValidation.XmlMalformed` error, even if the DTD is benign. This is intentional.
- XSD schemas that use `xs:import` with remote URIs will have their imports silently ignored (because `XmlResolver = null`). Developers must download and register imported schemas locally.

### Rejected alternatives

- **Opt-in secure (match BCL default):** Rejected. Requires developers to know and apply the correct settings. Experience shows this is frequently omitted.
- **Configurable Anti-XXE via options:** Rejected. Providing an `AllowDtd = true` escape hatch creates a footgun. Any legitimate use case (e.g., legacy internal XML with DTD) should process through a separate, explicitly unsecured pipeline — not through this library.
