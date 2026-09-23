# ADR-005: No Schematron Support in This Package

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

[Schematron](https://en.wikipedia.org/wiki/Schematron) (ISO/IEC 19757-3) is an XML validation language based on XPath assertions, distinct from W3C XML Schema (XSD). Some teams use both: XSD for structural validation and Schematron for business-rule validation.

The adjacent library `Schematron` by devlooped offers Schematron-based validation for .NET.

## Decision

**Schematron validation is intentionally excluded from `EricksonLopez.Xml.Validation`.**

If Schematron support is warranted in the future, it would be implemented as a **separate package**: `EricksonLopez.Xml.Schematron`.

## Rationale

1. **Different validation language:** Schematron uses XPath-based assertions, not XSD grammar rules. Integrating both in one package creates an unfocused API surface.

2. **Different runtime requirements:** Schematron validation typically requires an XSLT processor. This would introduce a significant runtime dependency that is not needed for XSD-only consumers.

3. **Package boundary principle:** This package's purpose is "XSD schema validation with Anti-XXE, Result<T>, and DI." Adding Schematron would violate the single-responsibility principle at the package level.

4. **Market reality:** Schematron and XSD address different problems. Most .NET applications that need XSD validation do not need Schematron, and vice versa.

5. **Existing competition:** The `Schematron` by devlooped library already exists and is maintained. There is no competitive gap to fill.

## Consequences

### Positive

- Package stays focused and lightweight.
- No XSLT processor dependency.
- Clear differentiation: this package = XSD validation; future `EricksonLopez.Xml.Schematron` = Schematron.

### Negative

- Teams using both XSD and Schematron validation must take two separate packages.
- No unified validation pipeline across both languages in a single `AddXmlValidation()` call.
  - Mitigation: A future `EricksonLopez.Xml.Validation.Combined` meta-package could compose both.

### Rejected alternatives

- **Integrate Schematron into this package:** Rejected. Scope creep. XSLT dependency. Confusing API surface.
- **Add Schematron to this package as optional feature:** Rejected. Optional features with heavy dependencies create packaging and trimming complications.
