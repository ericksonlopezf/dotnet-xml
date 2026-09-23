# ADR-004: XSD Warnings Configurable, Disabled by Default

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

The .NET BCL `XmlReaderSettings.ValidationEventHandler` fires for both `XmlSeverityType.Error` and `XmlSeverityType.Warning`. XSD warnings are non-fatal schema issues such as:

- `xs:any` with `namespace='##any'` creating ambiguous content models.
- Deprecated schema features.
- Schema constructs that are technically valid but may not behave as intended.

Warnings do **not** indicate that the XML is invalid — they indicate potential schema design issues.

In v1.0.0 of this library, all warnings were silently ignored (only errors were captured). The audit (2026-09-02) identified this as FALSE PARITY #2 vs. the BCL.

## Decision

In `EricksonLopez.Xml.Validation` (v1.0.0), warnings are configurable via `XmlValidationOptions`:

```csharp
public sealed class XmlValidationOptions
{
    // When true (or when TreatWarningsAsErrors is true), XSD warnings are
    // included in the Result error message, prefixed with "[Warning]".
    // getter: _includeWarnings || _treatWarningsAsErrors
    public bool IncludeWarnings { get; set; }

    // When true, warnings cause validation to fail and are automatically captured
    // (IncludeWarnings getter returns true regardless of its backing field).
    public bool TreatWarningsAsErrors { get; set; }
}
```

**Default behavior: `IncludeWarnings = false`** — warnings remain silently ignored.

Rationale for the default:
1. Most schemas in production have latent warnings that are not actionable.
2. Enabling warnings by default would be a breaking change for existing consumers.
3. Warnings are not validation failures — changing the pass/fail outcome requires explicit opt-in.

> [!IMPORTANT]
> Setting `TreatWarningsAsErrors = true` is **sufficient on its own** to cause warnings to be captured and to fail validation. The `IncludeWarnings` property getter is implemented as `_includeWarnings || _treatWarningsAsErrors`, so it automatically returns `true` whenever `TreatWarningsAsErrors` is `true` — regardless of whether `IncludeWarnings` was explicitly set. You do **not** need to set `IncludeWarnings = true` separately when using `TreatWarningsAsErrors = true`.

## Consequences

### Positive

- Full parity with BCL `XmlSeverityType.Warning` reporting.
- Configurable without changing the library API surface (just an option).
- Non-breaking: defaults preserve existing behavior.

### Negative

- When `IncludeWarnings = false` (default), schema warnings are invisible. Developers must opt-in to see them.
- Warning messages are appended to the error description string (same field as errors). No separate warning collection in `Result<bool>`.
  - A future `Result<ValidationReport>` (planned for v2.0) would provide distinct `Errors` and `Warnings` collections.

### Rejected alternatives

- **Always capture warnings:** Rejected. Would be a breaking change for consumers relying on clean pass/fail behavior.
- **Separate `IList<string> Warnings` out-param:** Rejected. Does not fit the `Result<T>` return type contract. Deferred to v2.0 `Result<ValidationReport>`.
- **Warning-specific event:** Rejected. Adds complexity without benefit over the options pattern.
