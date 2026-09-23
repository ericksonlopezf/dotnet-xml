# Product Strategy — EricksonLopez.Xml.Validation

> **Version:** 1.0 | **Date:** 2026-09-02 | **Role:** Senior Product Strategist + Competitive Intelligence Analyst  
> **Input:** Functional Parity Audit + Source Code Inspection + .NET 10 BCL Public API

---

## 1. Product Context

### The Problem It Solves

`EricksonLopez.Xml.Validation` simultaneously addresses **three chronic problems** left to developers when using the .NET Base Class Library (`System.Xml.Schema`):

1. **Security Vulnerability by Default:** `System.Xml.Schema` is insecure by default (DTD processing permitted, external entity resolution enabled). Developers unfamiliar with OWASP XXE attack vectors frequently introduce critical vulnerabilities into production systems.
2. **Repetitive Boilerplate:** Every BCL validation implementation requires ~20 lines of setup (`XmlSchemaSet`, `XmlReaderSettings`, `ValidationEventHandler`, `Compile()`). This boilerplate is duplicated across repositories, increasing the risk of omitting vital security flags.
3. **Control Flow via Exceptions and Callbacks:** The BCL API relies on event callbacks and throws exceptions for malformed XML or validation errors, which disrupts functional architectures (Railway-Oriented Programming, CQRS, Mediator pipelines).

### Target Audience

Primary persona: **.NET Software Engineers and Architects in Enterprise Environments** who:
- Build ASP.NET Core web APIs and background Worker Services.
- Process third-party XML payloads (electronic invoices, EDI, B2B exchanges, banking formats).
- Use standard dependency injection (`Microsoft.Extensions.DependencyInjection`).
- Adopt functional error-handling patterns (`Result<T>`, Railway-Oriented Programming).
- Face stringent compliance, security, and OWASP audit requirements.

Secondary persona: Developers within the **`EricksonLopez.*` ecosystem** leveraging foundational packages like `EricksonLopez.Result` and `EricksonLopez.SharedKernel`.

### Product Category

**Infrastructure Abstraction NuGet Library** (opinionated security defaults and high-performance caching for XML schema validation).

### Primary Use Cases

1. **Electronic Invoicing:** Validating XML invoices (UBL 2.1, CFDI, PEPPOL BIS) against precompiled XSD schemas in REST endpoints.
2. **Financial Exchanges:** Validating ISO 20022 (`pain.001`, `pacs.008`) messaging payloads in banking backends.
3. **Ingestion Pipelines:** Validating high-throughput XML streams before deserialization.
4. **Queue & Event Consumers:** Defensive perimeter validation within message consumers before message processing.

---

## 2. Competitive Landscape

| Competitor | Type | Real Threat | Strategic Position |
|---|---|---|---|
| `System.Xml.Schema` (BCL) | **Primary Substitute** | **HIGH** (freely available, standard) | Win on security-by-default, zero boilerplate, and `Result<T>` DX. |
| `Schematron` by devlooped | **Adjacent** | Low (validates ISO Schematron, not XSD) | Complementary standard; no direct rivalry. |
| `XmlFluentValidator` | **Adjacent** | Very Low (programmatic rules, not XSD) | Niche; different validation paradigm. |
| Saxon-EE / SaxonCS | **Framework Alternative** | Low (commercial, XSD 1.1 focus) | Enterprise commercial niche; heavy footprint. |

> **Critical Strategic Insight:** The true competition is not another NuGet package—it is the pattern of developers continuing to write manual, error-prone BCL boilerplate.

---

## 3. Core Value Proposition

```text
EricksonLopez.Xml.Validation =
    Zero-Boilerplate XSD Validation
    + Unconditional Anti-XXE Defense
    + Thread-Safe Precompiled Schema Cache ($O(1)$)
    + Structured Railway-Oriented Errors (Result<bool>)
    + Native AOT & Trim Safe Out-of-the-Box
    + First-Class Microsoft DI Integration
```

---

## 4. Feature Classification & Prioritization

### 4.1 Core Differentiators (Hard-to-Replicate Advantages)
- **Unconditional Anti-XXE:** Cannot be accidentally disabled through configuration. This provides an absolute guarantee for security and compliance audits.
- **Result Pattern (`Result<bool>`):** Eliminates expensive runtime exception throws on validation failures, aligning with high-throughput microservices.
- **First-Class DI:** `services.AddXmlValidation()` configures singleton lifetimes and seamless integration with `ILogger`.

### 4.2 Competitive Parity (Completed)
- Multi-modal inputs: `string`, `Stream`, `ReadOnlySpan<byte>`, and `async Stream`.
- Line and position reporting from `IXmlLineInfo`.
- Configurable warning reporting via `XmlValidationOptions`.
- Zero-allocation diagnostic logging via compile-time `[LoggerMessage]`.

### 4.3 Intentionally Excluded Features (Architectural Tradeoffs)
- **Schematron (ISO/IEC 19757-3):** Excluded to preserve zero third-party dependencies, Native AOT compliance, and minimal memory footprint.
- **Push-Model `XmlSchemaValidator`:** Excluded; the pull-based streaming model satisfies 99.9% of enterprise requirements.
- **XSD Dynamic Type Inference:** Excluded; design-time concerns belong to build tools, not runtime engines.

---

## 5. Adoption & Go-To-Market Roadmap

1. **Developer Trust & Credibility:**
   - Maintain 100% test pass rate across all supported .NET runtimes (.NET 8.0, 9.0, 10.0).
   - Continuous verification of Native AOT trimming via dedicated smoke tests.
2. **Interactive Onboarding (The Showcase):**
   - Provide an 11-level executable showcase (`samples/EricksonLopez.Xml.Showcase`) demonstrating real enterprise schemas (UBL 2.1, ISO 20022).
3. **Comprehensive Documentation:**
   - 13 detailed developer guides, 17 formal ADRs, and complete API references.

---

*End of Product Strategy — EricksonLopez.Xml.Validation*
