# Troubleshooting Guide — EricksonLopez.Xml.Validation

Diagnosing errors, error codes, and resolving common operational issues.

---

## 1. Error: `XmlValidation.SchemaNotRegistered`

### Symptom
Validation fails with error code `XmlValidation.SchemaNotRegistered` and error type `ErrorType.NotFound`.

### Root Cause
`validator.Validate(...)` was called with a `targetNamespace` that has not been preloaded into the active `IXmlSchemaCache` instance.

### Resolution
1. Verify that the `targetNamespace` passed to `Validate(...)` matches the schema namespace **exact character-for-character** (case-sensitive, trailing slashes included):
   ```csharp
   // Namespace strings must be identical
   cache.RegisterSchema("https://ericksonlopez.dev/schemas/orders", xsdContent);
   validator.Validate(xml, "https://ericksonlopez.dev/schemas/orders");
   ```
2. Ensure that schema registration logic (`cache.RegisterSchemasFromDirectory(...)`) executes successfully before endpoints begin accepting incoming requests.

---

## 2. Error: `For security reasons DTD is prohibited in this XML document`

### Symptom
The validation result fails with error code `XmlValidation.XmlMalformed`, and the error description states that DTD processing is prohibited.

### Root Cause
The incoming XML payload contains a `<!DOCTYPE ...>` declaration. This may indicate an XXE injection attempt, an XML entity expansion attack ("Billion Laughs"), or a legacy XML document using DTD entities.

### Resolution
1. **If from an external or untrusted source:** The rejection is intentional. The library has successfully neutralized a potential vulnerability (ADR-001).
2. **If from a legacy internal system using DTD entities (`&myEntity;`):** Replace DTD entity definitions with literal values or standard XSD schema types.

---

## 3. Schema Entries Overwritten with Sequential Registrations

### Symptom
When registering multiple schemas for the same `targetNamespace` individually, only the definitions from the last registered schema are available during validation.

### Root Cause
`XmlSchemaCache` uses `targetNamespace` as a 1:1 key in its internal `ConcurrentDictionary<string, SchemaCacheEntry>`. When multiple individual calls to `RegisterSchema(...)` or `RegisterSchemaFile(...)` pass the same `targetNamespace`, each subsequent registration replaces the prior precompiled entry (see [ADR-003](adr/adr-003-namespace-as-cache-key.md)).

> [!NOTE]
> When bulk loading via `RegisterSchemasFromDirectory(...)`, the library automatically groups all `.xsd` files sharing the same `targetNamespace` and compiles them together into a unified `XmlSchemaSet` (see [ADR-003](adr/adr-003-namespace-as-cache-key.md) and [Architecture Guide](architecture-guide.md#41-type-hierarchy--member-design)).

### Resolution
1. If your schemas are split across multiple files on disk, use `cache.RegisterSchemasFromDirectory(path)` to automatically aggregate and compile all fragments into a single unified `XmlSchemaSet`.
2. If registering programmatically, consolidate fragmented schemas into a single root `.xsd` document, or pass a unified schema definition.

---

## 4. Warnings Are Not Reported in `Result<bool>`

### Symptom
An XSD schema produces warnings during validation, but `result.IsSuccess` remains `true` and the warnings do not appear in `result.Error.Description`.

### Root Cause
By default, `XmlValidationOptions.IncludeWarnings` is `false` (ADR-004).

### Resolution
Enable warning reporting in the dependency injection configuration:
```csharp
builder.Services.AddXmlValidation(options =>
{
    options.IncludeWarnings = true;
    options.TreatWarningsAsErrors = true; // Set to true if warnings should cause validation failure
});
```

> [!NOTE]
> When `TreatWarningsAsErrors = true` is set, `IncludeWarnings` automatically evaluates to `true`, ensuring that schema warnings are captured by the reader and cause validation failure with full diagnostic details.
