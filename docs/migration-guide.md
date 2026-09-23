# Migration Guide — From Direct BCL to EricksonLopez.Xml.Validation

Step-by-step instructions for migrating legacy XML validation code based directly on `System.Xml.Schema` (.NET BCL) to `EricksonLopez.Xml.Validation`.

---

## 1. Pattern Comparison

### Before: Legacy BCL Approach (Boilerplate, Error-Prone, Insecure Defaults)

```csharp
// ❌ LEGACY CODE:
// 1. Repetitive compilation of XmlSchemaSet on every invocation
var schemas = new XmlSchemaSet();
schemas.Add("https://example.com/orders", "orders.xsd");
schemas.Compile();

// 2. Anti-XXE security flags easily omitted
var settings = new XmlReaderSettings
{
    ValidationType = ValidationType.Schema,
    Schemas = schemas
};

// 3. Expensive runtime exceptions used for validation control flow
try
{
    using var reader = XmlReader.Create(new StringReader(xml), settings);
    while (reader.Read()) { }
    Console.WriteLine("Valid");
}
catch (XmlSchemaValidationException ex)
{
    Console.WriteLine($"Invalid: {ex.Message}");
}
catch (XmlException ex)
{
    Console.WriteLine($"Malformed: {ex.Message}");
}
```

---

### After: Modern Pattern with EricksonLopez.Xml.Validation

```csharp
// ✔ MODERN PATTERN:
// 1. Schema registered once in the thread-safe singleton cache
var cache = new XmlSchemaCache();
cache.RegisterSchemaFile("https://example.com/orders", "orders.xsd");

var validator = new XmlSchemaValidator(cache);

// 2. High-performance, XXE-protected validation without exceptions
var result = validator.Validate(xml, "https://example.com/orders");

if (result.IsSuccess)
{
    Console.WriteLine("Valid");
}
else
{
    // Clean, typed error handling
    Console.WriteLine($"Invalid [{result.Error.Code}]: {result.Error.Description}");
}
```

---

## 2. Step-by-Step Migration Checklist

1. **Replace Ad-Hoc `XmlSchemaSet` Instantiations:** Add `services.AddXmlValidation()` to your dependency injection configuration during host startup.
2. **Remove `try/catch (XmlSchemaValidationException)` Blocks:** Convert conditional branches to check `if (result.IsFailure)`.
3. **Remove Manual Anti-XXE Sanitization Code:** The library enforces `DtdProcessing.Prohibit` and `XmlResolver = null` automatically on every code path.
4. **Update Endpoint Error Responses:** Map `result.Error.Code` to standard HTTP status codes (`400 Bad Request` for `XmlMalformed`, `422 Unprocessable Entity` for `SchemaViolation`, `404 Not Found` for `SchemaNotRegistered`).
