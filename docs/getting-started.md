# Getting Started — EricksonLopez.Xml.Validation

A step-by-step developer guide for architects and software engineers integrating XSD schema validation into .NET production systems.

---

## 1. Prerequisites

- **.NET SDK:** Version 8.0, 9.0, or 10.0 installed.
- **NuGet Package:** `EricksonLopez.Xml.Validation`.

Install the package via the .NET CLI:

```bash
dotnet add package EricksonLopez.Xml.Validation
```

---

## 2. Core Concepts

The package is architected around two primary interfaces:

1. **`IXmlSchemaCache`**: A thread-safe, precompiled schema registry.
   - Backed by `ConcurrentDictionary<string, SchemaCacheEntry>` (each entry encapsulates a compiled `XmlSchemaSet` and a pre-indexed set of global elements).
   - Compiles schemas atomically while isolating external entity definitions (Anti-XXE).
   - Indexed uniquely by the target XML namespace (`targetNamespace`).

2. **`IXmlSchemaValidator`**: The stateless validation engine.
   - Evaluates documents against precompiled schemas.
   - Returns structured `Result<bool>` from `EricksonLopez.Result` (zero exceptions for validation failures).
   - Emits zero-allocation diagnostic logging via compile-time `[LoggerMessage]` delegates when wired to `ILogger`.

---

## 3. Schema Registration

Schemas can be loaded into `IXmlSchemaCache` through multiple mechanisms:

### A. From Files on Disk
```csharp
cache.RegisterSchemaFile(
    targetNamespace: "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2",
    filePath: "/etc/schemas/invoice.xsd");
```

### B. Bulk Directory Registration
```csharp
// Recursively scans the directory and compiles all matching *.xsd files
int total = cache.RegisterSchemasFromDirectory("/etc/schemas", "*.xsd");
Console.WriteLine($"Compiled {total} schemas successfully.");
```

### C. From Streams or In-Memory Strings
```csharp
using var stream = File.OpenRead("schema.xsd");
cache.RegisterSchema("https://example.com/schema", stream);
```

---

## 4. Input Modalities

`IXmlSchemaValidator` provides 4 distinct input modalities tailored to different workloads:

| Modality | Method Signature | Recommended Scenario |
|---|---|---|
| **String** | `Validate(string, string)` | Small in-memory XML payloads (< 100 KB) |
| **Sync Stream** | `Validate(Stream, string)` | Local files or pre-buffered memory streams |
| **ReadOnlySpan&lt;byte&gt;** | `Validate(ReadOnlySpan<byte>, string)` | Socket buffers, queues, reduced-allocation hot paths (avoids string heap allocation) |
| **Async Stream** | `ValidateAsync(Stream, string, CancellationToken)` | HTTP request bodies (`HttpRequest.Body`) and network streams |

---

## 5. Interpreting Results

Validation results use the `Result<bool>` type to enforce clean error handling:

```csharp
var result = validator.Validate(xml, targetNamespace);

if (result.IsSuccess)
{
    // The document strictly conforms to the schema
    ProcessOrder(xml);
}
else
{
    // Handle specific error conditions cleanly without try/catch
    switch (result.Error.Code)
    {
        case "XmlValidation.SchemaNotRegistered":
            // 404: The target namespace has not been preloaded into the cache
            Console.WriteLine($"Missing schema: {result.Error.Description}");
            break;

        case "XmlValidation.SchemaViolation":
            // 422: Schema rule violation (includes line and character coordinates)
            Console.WriteLine($"Schema error: {result.Error.Description}");
            break;

        case "XmlValidation.XmlMalformed":
            // 400: Malformed XML syntax or neutralized XXE attack attempt
            Console.WriteLine($"Syntax/Security error: {result.Error.Description}");
            break;
    }
}
```

---

## 6. Next Steps

- Explore the [Quick Start Guide](quick-start.md) for a 5-minute setup.
- Review real-world examples in the [Cookbook](cookbook.md).
- Run the interactive [Showcase Reference Project](../samples/EricksonLopez.Xml.Showcase).
