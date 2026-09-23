# ADR-002: Result\<T\> Instead of Exceptions for Validation Failures

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

XML validation failures (schema violations, malformed XML, unregistered schemas) are **expected, recoverable conditions** — not exceptional circumstances. There are two primary patterns in .NET for communicating these outcomes:

1. **Exception-based:** throw `XmlSchemaValidationException`, catch at call site.
2. **Result-based (Railway Oriented Programming):** return a `Result<T>` discriminated union.

The `System.Xml.Schema` BCL uses both patterns: it raises `ValidationEventHandler` callbacks for schema errors and can throw `XmlException` for malformed XML.

## Decision

All validation methods return `Result<bool>` from the `EricksonLopez.Result` package instead of throwing exceptions for expected failure scenarios:

```csharp
// Schema violation → Result.Failure with Error.Validation
Result<bool> Validate(string xml, string targetNamespace);

// Schema not found → Result.Failure with Error.NotFound
// Malformed XML    → Result.Failure with Error.Validation
// Valid XML        → Result.Success(true)
```

Exception categories:
- **`ArgumentNullException`:** thrown for null arguments (programming error, not validation failure).
- **`OperationCanceledException`:** propagated from `ValidateAsync` when token is cancelled (framework convention).
- **All other XML errors:** mapped to `Result.Failure`.

## Consequences

### Positive

- Call sites cannot silently ignore validation failures (compilation error if `Result<T>` is not checked).
- Integrates natively with Railway Oriented Programming pipelines and `EricksonLopez.Mediator` behaviors.
- No performance cost of exception creation/unwinding for the common validation-failure path.
- Error codes (`XmlValidation.SchemaViolation`, `XmlValidation.SchemaNotRegistered`, `XmlValidation.XmlMalformed`) are string constants — easy to switch/match without catching exception types.
- Consistent with the rest of the `EricksonLopez.*` ecosystem.

### Negative

- Requires adopting `EricksonLopez.Result` as a dependency (transitively brings in the `Result<T>` type).
- Developers unfamiliar with Railway Oriented Programming must learn the pattern.
- Code that expects exceptions (e.g., legacy try/catch-heavy codebases) requires adaptation.

### Rejected alternatives

- **Throwing `XmlSchemaValidationException`:** Rejected. Encourages try/catch swallowing, hides failure paths from callers, and incurs stack-unwinding cost for expected outcomes.
- **Custom checked-exception pattern:** Rejected. C# has no compiler-enforced checked exceptions. Result<T> achieves the same forcing function at the type level.
- **Returning null/bool:** Rejected. Provides no error detail and requires separate out-param or second call.
