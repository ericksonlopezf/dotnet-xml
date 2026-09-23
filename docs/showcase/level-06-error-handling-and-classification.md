# Level 06: Error Taxonomy & Anti-XXE Defenses

## Overview

Traditional XML validation relies on throwing `XmlSchemaValidationException` and `XmlException`. `EricksonLopez.Xml.Validation` adopts Railway-Oriented Programming, returning deterministic `Result<bool>` errors categorized into a standardized failure taxonomy with zero control-flow exceptions.

---

## 1. Error Taxonomy Matrix

| Error Code | Category | RFC 9457 HTTP | Cause |
|---|---|:---:|---|
| `XmlValidation.SchemaNotRegistered` | `ErrorType.NotFound` | `404 Not Found` | The requested `targetNamespace` was not registered in `IXmlSchemaCache`. |
| `XmlValidation.SchemaViolation` | `ErrorType.Validation` | `422 Unprocessable Entity` | Document violates XSD constraints (invalid types, missing elements, regex mismatch). |
| `XmlValidation.XmlMalformed` | `ErrorType.Validation` | `400 Bad Request` | Document is syntactically invalid or contains prohibited DTD declarations. |

---

## 2. Anti-XXE & Billion Laughs Neutralization

The XML External Entity (XXE) attack (OWASP Top 10) targets XML parsers that resolve external DTD entities, risking local file disclosure and SSRF:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE Order [
  <!ELEMENT Order ANY >
  <!ENTITY xxe SYSTEM "file:///etc/passwd" >]>
<Order xmlns="https://example.com/schema">
  <OrderId>&xxe;</OrderId>
</Order>
```

### Defense in Depth Invariant
`XmlSchemaValidator` enforces Anti-XXE defaults unconditionally:
1. `DtdProcessing = DtdProcessing.Prohibit`
2. `XmlResolver = null`

Any document containing `<!DOCTYPE ...>` is aborted at the tokenizer level with `XmlValidation.XmlMalformed`, guaranteeing **zero entity expansion and zero external HTTP/file resolution**.

---

## 3. Warning Escalation (`TreatWarningsAsErrors`)

For regulatory compliance (UBL 2.1, ISO 20022), non-fatal schema warnings can be captured and escalated to validation failures:

```csharp
var options = new XmlValidationOptions
{
    IncludeWarnings = true,
    TreatWarningsAsErrors = true
};
var validator = new XmlSchemaValidator(cache, options);
```

---

## 4. File System Registration Errors (`XmlSchemaCacheExtensions`)

Extension methods `RegisterSchemaFile` and `RegisterSchemasFromDirectory` throw typed BCL exceptions — **not** `Result<T>` — since these are configuration-time failures that should cause fast-fail startup, not runtime degradation:

| Method | Exception | Trigger |
|---|---|---|
| `RegisterSchemaFile` | `FileNotFoundException` | The specified `.xsd` file path does not exist |
| `RegisterSchemaFile` | `XmlException` | The file contains malformed XML |
| `RegisterSchemaFile` | `XmlSchemaException` | The file contains invalid XSD schema definitions |
| `RegisterSchemasFromDirectory` | `DirectoryNotFoundException` | The specified directory path does not exist |
| `RegisterSchemasFromDirectory` | `XmlException` | Any discovered `.xsd` file contains malformed XML |

```csharp
// Fast-fail pattern during IHostedService.StartAsync
try
{
    cache.RegisterSchemaFile(ns, "/schemas/invoice.xsd");
}
catch (FileNotFoundException ex)
{
    // Log and abort startup — schema catalog is incomplete
    logger.LogCritical(ex, "Required XSD schema missing. Startup aborted.");
    throw; // Host will not start
}
```

> **Design Rationale:** Registration errors are configuration errors — not recoverable at runtime. They should prevent the service from starting rather than silently degrading validation behavior.

---

## Code Reference
- [`Level06ErrorHandlingAndClassification.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level06ErrorHandlingAndClassification.cs)
