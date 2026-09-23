# ADR-012: Structured Error Taxonomy and Diagnostic Error Codes

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

Callers need programmatic discrimination between distinct failure modes:
1. XML syntax corruption (unclosed tags, encoding mismatches).
2. Schema non-compliance (missing required element, type mismatch).
3. Schema registry lookup miss (target namespace not found).
4. System/Internal errors.

Generic error strings force callers into fragile string matching.

## Decision

We establish a canonical, structured error catalog under the `XmlValidation` domain hierarchy:
- `XmlValidation.SchemaNotRegistered`: The requested target namespace has not been registered in the schema cache.
- `XmlValidation.XmlMalformed`: The document violates standard XML 1.0 grammar or contains prohibited DTD declarations.
- `XmlValidation.SchemaViolation`: The document structure or values violate the compiled XSD schema definition.

## Consequences

### Positive
- Callers can match on error codes programmatically (`error.Code == "XmlValidation.SchemaNotRegistered"`).
- Standardized error codes map cleanly to HTTP status codes (400 Bad Request vs 404 Not Found vs 422 Unprocessable Entity).
- Detailed metadata (line number, column number, message) remains intact in `error.Description`.

### Negative
- Requires maintaining backward compatibility of error code strings across major versions.
