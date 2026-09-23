# Level 01: Getting Started & Schema Registration

## 1. Installation & Service Registration

`EricksonLopez.Xml.Validation` integrates cleanly with Microsoft Dependency Injection or can be instantiated directly as a standalone component.

```bash
dotnet add package EricksonLopez.Xml.Validation
```

### Dependency Injection Registration

```csharp
using EricksonLopez.Xml.Validation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register the precompiled schema cache and validator with default singleton lifetime
services.AddXmlValidation();

var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<IXmlSchemaValidator>();
var cache = provider.GetRequiredService<IXmlSchemaCache>();
```

---

## 2. Precompiled Schema Cache Registration

Schemas must be registered once during application startup or warm-up. Registration compiles and locks the `XmlSchemaSet` into a thread-safe in-memory cache:

```csharp
const string TargetNamespace = "https://schemas.ericksonlopez.dev/invoicing/v1";
const string InvoiceXsd = """
    <?xml version="1.0" encoding="utf-8"?>
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
               targetNamespace="https://schemas.ericksonlopez.dev/invoicing/v1"
               xmlns="https://schemas.ericksonlopez.dev/invoicing/v1"
               elementFormDefault="qualified">
      <xs:element name="Invoice">
        <xs:complexType>
          <xs:sequence>
            <xs:element name="InvoiceId" type="xs:string" />
            <xs:element name="Amount" type="xs:decimal" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:schema>
    """;

// Register via raw string, Stream, or ReadOnlySpan<byte>
cache.RegisterSchema(TargetNamespace, InvoiceXsd);
```

---

## 3. Validating Documents with Result<T>

Validation methods return a `Result<bool>` from `EricksonLopez.Result`. No exceptions are thrown for validation errors or malformed payloads:

```csharp
const string ValidXml = """
    <Invoice xmlns="https://schemas.ericksonlopez.dev/invoicing/v1">
      <InvoiceId>INV-2026-001</InvoiceId>
      <Amount>1450.50</Amount>
    </Invoice>
    """;

Result<bool> result = validator.Validate(ValidXml, TargetNamespace);

if (result.IsSuccess)
{
    Console.WriteLine("Document is valid.");
}
else
{
    Console.Error.WriteLine($"Validation failed: [{result.Error.Code}] {result.Error.Description}");
}
```
