# Quick Start — EricksonLopez.Xml.Validation

Learn how to validate XML documents against precompiled XSD schemas in under 5 minutes with unconditional Anti-XXE security.

---

## 1. Installation

Install the package into your .NET project:

```bash
dotnet add package EricksonLopez.Xml.Validation
```

---

## 2. Minimal Standalone Usage (No Dependency Injection)

```csharp
using System;
using EricksonLopez.Xml.Validation;

// 1. Instantiate the cache and register your XSD schema
var cache = new XmlSchemaCache();
cache.RegisterSchema(
    targetNamespace: "https://ericksonlopez.dev/schemas/orders",
    xsdContent: """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="https://ericksonlopez.dev/schemas/orders"
                   xmlns="https://ericksonlopez.dev/schemas/orders"
                   elementFormDefault="qualified">
          <xs:element name="Order">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="OrderId" type="xs:string" />
                <xs:element name="Quantity" type="xs:int" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """);

// 2. Instantiate the validator
var validator = new XmlSchemaValidator(cache);

// 3. Validate an XML document
string xml = """
    <Order xmlns="https://ericksonlopez.dev/schemas/orders">
      <OrderId>ORD-101</OrderId>
      <Quantity>5</Quantity>
    </Order>
    """;

var result = validator.Validate(xml, "https://ericksonlopez.dev/schemas/orders");

if (result.IsSuccess)
{
    Console.WriteLine("Document is valid!");
}
else
{
    Console.WriteLine($"Validation failed [{result.Error.Code}]: {result.Error.Description}");
}
```

---

## 3. Usage with Microsoft Dependency Injection

In ASP.NET Core web applications or Worker Services:

```csharp
using System.IO;
using EricksonLopez.Xml.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register cache and validator singletons in DI
builder.Services.AddXmlValidation();

var app = builder.Build();

// Preload schemas during application bootstrap
var cache = app.Services.GetRequiredService<IXmlSchemaCache>();
cache.RegisterSchemasFromDirectory("./Schemas");

// Inject IXmlSchemaValidator directly into endpoints
app.MapPost("/api/orders", async (Stream body, IXmlSchemaValidator validator) =>
{
    var result = await validator.ValidateAsync(body, "https://ericksonlopez.dev/schemas/orders");
    
    return result.IsSuccess 
        ? Results.Ok("Valid Order") 
        : Results.UnprocessableEntity(result.Error.Description);
});

app.Run();
```

---

## Next Steps

- Consult the [In-Depth Getting Started Guide](getting-started.md).
- Browse practical enterprise examples in the [Cookbook](cookbook.md).
- Run the interactive [Showcase Reference Project](../samples/EricksonLopez.Xml.Showcase).
