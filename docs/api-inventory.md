# Public API Inventory — EricksonLopez.Xml.Validation

> **Version:** 1.0.0 | **Supported Frameworks:** .NET 8.0, 9.0, 10.0 | **Native AOT:** Verified (`IsAotCompatible=true`)  
> **Source of Truth:** `src/EricksonLopez.Xml.Validation/` | **Last Sync:** 2026-09-23

This document provides the authoritative and exhaustive inventory of the public API surface for `EricksonLopez.Xml.Validation`. Per repository governance and architectural invariants, this inventory is the **single source of truth** for all code, tests, documentation, cookbooks, and the executable Showcase reference implementation.

---

## 1. Master Public API Inventory Table

| Name | Namespace | Responsibility | Dependencies | Use Cases | Complexity Level | Existing Example |
|---|---|---|---|---|---|---|
| `IXmlSchemaCache` | `EricksonLopez.Xml.Validation` | Contract for thread-safe in-memory registration and caching of precompiled XSD schema sets (`XmlSchemaSet`) indexed by `targetNamespace`. | `System.IO`, `System.Xml.Schema` | One-time schema registration at startup; concurrent $O(1)$ lookup without recompilation. | Basic / Intermediate | Yes (Levels 1, 2, 3, 4, 5, 7, 8, 9, 10) |
| `XmlSchemaCache` | `EricksonLopez.Xml.Validation` | Sealed, thread-safe implementation of `IXmlSchemaCache` backed by `ConcurrentDictionary`, enforcing strict Anti-XXE during compilation. | `System.Xml`, `System.Xml.Schema`, `System.Collections.Concurrent` | Central schema registry in production and test environments; registered as singleton in DI container. | Basic / Intermediate | Yes (Levels 1, 2, 3, 4, 5, 7, 8, 9, 10) |
| `XmlSchemaCacheExtensions` | `EricksonLopez.Xml.Validation` | Extension methods for `IXmlSchemaCache` enabling loading from physical files and recursive directory scanning. | `System.IO`, `System.Xml.Schema` | Bulk preloading of schema catalogs (e.g., UBL 2.1, ISO 20022) from disk-mounted volumes. | Intermediate | Yes (Levels 3, 4, 5, 6) |
| `IXmlSchemaValidator` | `EricksonLopez.Xml.Validation` | Contract for validating XML documents against precompiled schemas across multiple input modalities (`string`, `Stream`, `ReadOnlySpan<byte>`, `async Stream`), returning `Result<bool>`. | `EricksonLopez.Result`, `System.IO`, `System.Threading` | XML payload validation in API controllers, minimal endpoints, queue consumers, and perimeter gateways. | Basic / Advanced | Yes (Levels 1, 2, 3, 4, 5, 6, 7, 8, 9, 10) |
| `XmlSchemaValidator` | `EricksonLopez.Xml.Validation` | Production validation engine with unconditional Anti-XXE defenses, zero-allocation `[LoggerMessage]` diagnostic logging, and configurable options support. | `IXmlSchemaCache`, `XmlValidationOptions`, `ILogger<XmlSchemaValidator>` | High-concurrency validation, prevention of XXE injections and entity expansion attacks (Billion Laughs). | Intermediate / Advanced | Yes (Levels 1, 2, 3, 4, 5, 6, 7, 8, 9, 10) |
| `XmlValidationOptions` | `EricksonLopez.Xml.Validation` | Validation pipeline configuration options: warning handling, DoS character limits, error accumulation caps, and inline schema policies. | BCL (`System`) | Regulatory severity tuning, DoS hardening, and memory limit controls in microservices. | Intermediate | Yes (Levels 2, 6) |
| `XmlValidationServiceCollectionExtensions` | `EricksonLopez.Xml.Validation` | Extension methods for `IServiceCollection` registering `IXmlSchemaCache` and `IXmlSchemaValidator` as Singletons in the Microsoft DI container. | `Microsoft.Extensions.DependencyInjection` | Idiomatic dependency bootstrapping in ASP.NET Core applications, Worker Services, and .NET console apps. | Basic | Yes (Levels 2, 8) |

---

## 2. Public Methods, Constructors, and Properties Detail

### 2.1 [`IXmlSchemaCache`](../src/EricksonLopez.Xml.Validation/IXmlSchemaCache.cs)

```csharp
namespace EricksonLopez.Xml.Validation;

public interface IXmlSchemaCache
{
    void RegisterSchema(string targetNamespace, string xsdContent);
    void RegisterSchema(string targetNamespace, Stream xsdStream);
    void RegisterSchema(string targetNamespace, ReadOnlySpan<byte> utf8Xsd);
    bool ContainsSchema(string targetNamespace);
    int Count { get; }
    bool IsRootElementDeclared(string targetNamespace, string localName, string namespaceUri);
    void Clear();
}
```

| Member | Kind | Signature | Return | Documented Exceptions | Showcase Level |
|---|---|---|---|---|---|
| `RegisterSchema` | Method | `(string targetNamespace, string xsdContent)` | `void` | `ArgumentNullException`, `XmlException`, `XmlSchemaException` | Levels 1, 2, 7, 8, 9, 10 |
| `RegisterSchema` | Method | `(string targetNamespace, Stream xsdStream)` | `void` | `ArgumentNullException`, `XmlException`, `XmlSchemaException` | Levels 1, 8 |
| `RegisterSchema` | Method | `(string targetNamespace, ReadOnlySpan<byte> utf8Xsd)` | `void` | `ArgumentNullException`, `XmlException`, `XmlSchemaException` | Levels 1, 8 |
| `ContainsSchema` | Method | `(string targetNamespace)` | `bool` | `ArgumentNullException` | Levels 1, 8 |
| `Count` | Property | `get` | `int` | None | Levels 1, 2, 8 |
| `IsRootElementDeclared` | Method | `(string targetNamespace, string localName, string namespaceUri)` | `bool` | `ArgumentNullException` | Levels 1, 2, 8 |
| `Clear` | Method | `()` | `void` | None | Levels 1, 8 |

> [!NOTE]
> `TryGetSchemaSet` intentionally belongs to the internal `IXmlSchemaSetProvider` interface (XML-API-001) to protect the mutability of precompiled schemas from unauthorized external access.

---

### 2.2 [`XmlSchemaCache`](../src/EricksonLopez.Xml.Validation/XmlSchemaCache.cs)

```csharp
namespace EricksonLopez.Xml.Validation;

// Public declaration
public sealed class XmlSchemaCache : IXmlSchemaCache, IXmlSchemaSetProvider
{
    public XmlSchemaCache();
}

// IXmlSchemaSetProvider is internal — not part of the public API surface.
// It enables XmlSchemaValidator to access compiled XmlSchemaSet instances
// internally without exposing mutable schema handles to consumer code.
// internal interface IXmlSchemaSetProvider
// {
//     bool TryGetSchemaSet(string targetNamespace, out XmlSchemaSet? schemaSet);
// }
```

| Member | Kind | Signature | Return | Behavior | Showcase Level |
|---|---|---|---|---|---|
| `.ctor` | Constructor | `()` | `XmlSchemaCache` | Initializes an empty cache backed by a `ConcurrentDictionary<string, SchemaCacheEntry>` with `StringComparer.Ordinal`. Each `SchemaCacheEntry` holds a compiled `XmlSchemaSet` and a pre-indexed `HashSet` of global elements. | Levels 1, 3, 4, 5, 6, 7, 8, 9, 10 |

> [!NOTE]
> `XmlSchemaCache` implements both the public `IXmlSchemaCache` (schema registration and safe query operations) and the `internal IXmlSchemaSetProvider` (internal schema retrieval for `XmlSchemaValidator`). The internal interface is an architectural encapsulation boundary (XML-API-001, [ADR-017](adr/adr-017-extensibility-boundary-and-schema-cache-contract-segregation.md)) and is not accessible to consumer code.

---

### 2.3 [`XmlSchemaCacheExtensions`](../src/EricksonLopez.Xml.Validation/XmlSchemaCacheExtensions.cs)

```csharp
namespace EricksonLopez.Xml.Validation;

public static class XmlSchemaCacheExtensions
{
    public static void RegisterSchemaFile(this IXmlSchemaCache cache, string targetNamespace, string filePath);
    public static int RegisterSchemasFromDirectory(this IXmlSchemaCache cache, string directoryPath, string searchPattern = "*.xsd");
}
```

| Member | Kind | Signature | Return | Documented Exceptions | Showcase Level |
|---|---|---|---|---|---|
| `RegisterSchemaFile` | Extension Method | `(this IXmlSchemaCache cache, string targetNamespace, string filePath)` | `void` | `ArgumentNullException`, `FileNotFoundException`, `XmlException`, `XmlSchemaException` | Level 3 |
| `RegisterSchemasFromDirectory` | Extension Method | `(this IXmlSchemaCache cache, string directoryPath, string searchPattern = "*.xsd")` | `int` | `ArgumentNullException`, `DirectoryNotFoundException`, `XmlException`, `XmlSchemaException` | Levels 4, 5, 6 |

---

### 2.4 [`IXmlSchemaValidator`](../src/EricksonLopez.Xml.Validation/IXmlSchemaValidator.cs)

```csharp
namespace EricksonLopez.Xml.Validation;

public interface IXmlSchemaValidator
{
    Result<bool> Validate(string xml, string targetNamespace);
    Result<bool> Validate(Stream xmlStream, string targetNamespace);
    Result<bool> Validate(ReadOnlySpan<byte> utf8Xml, string targetNamespace);
    Task<Result<bool>> ValidateAsync(Stream xmlStream, string targetNamespace, CancellationToken cancellationToken = default);
}
```

| Member | Kind | Signature | Return | Failure Behavior | Showcase Level |
|---|---|---|---|---|---|
| `Validate` | Method | `(string xml, string targetNamespace)` | `Result<bool>` | Throws `InvalidOperationException` if cache lacks schema provider; returns structured `Error` for validation failure. | Levels 1, 2, 3, 4, 6, 7, 8, 10 |
| `Validate` | Method | `(Stream xmlStream, string targetNamespace)` | `Result<bool>` | Throws `InvalidOperationException` if cache lacks schema provider; preserves stream open for downstream consumption. | Levels 4, 8 |
| `Validate` | Method | `(ReadOnlySpan<byte> utf8Xml, string targetNamespace)` | `Result<bool>` | Throws `InvalidOperationException` if cache lacks schema provider; zero intermediate heap string allocations. | Levels 4, 7, 8 |
| `ValidateAsync` | Method | `(Stream xmlStream, string targetNamespace, CancellationToken cancellationToken = default)` | `Task<Result<bool>>` | Throws `OperationCanceledException` on cancellation; throws `InvalidOperationException` if cache lacks provider; returns `Result<bool>`. | Levels 4, 5, 8, 9 |

---

### 2.5 [`XmlSchemaValidator`](../src/EricksonLopez.Xml.Validation/XmlSchemaValidator.cs)

```csharp
namespace EricksonLopez.Xml.Validation;

public sealed partial class XmlSchemaValidator : IXmlSchemaValidator
{
    public XmlSchemaValidator(IXmlSchemaCache schemaCache);
    public XmlSchemaValidator(IXmlSchemaCache schemaCache, XmlValidationOptions options, ILogger<XmlSchemaValidator>? logger = null);
}
```

| Member | Kind | Signature | Behavior | Showcase Level |
|---|---|---|---|---|
| `.ctor` | Constructor | `(IXmlSchemaCache schemaCache)` | Creates validator with default `XmlValidationOptions` and `NullLogger`. | Levels 1, 3, 4, 5, 7, 8, 9, 10 |
| `.ctor` | Constructor | `(IXmlSchemaCache schemaCache, XmlValidationOptions options, ILogger<XmlSchemaValidator>? logger = null)` | Creates validator with customized options and structured logger. | Levels 2, 6 |

---

### 2.6 [`XmlValidationOptions`](../src/EricksonLopez.Xml.Validation/XmlValidationOptions.cs)

```csharp
namespace EricksonLopez.Xml.Validation;

public sealed class XmlValidationOptions
{
    public XmlValidationOptions();
    public bool IncludeWarnings { get; set; }
    public bool TreatWarningsAsErrors { get; set; }
    public long MaxCharactersInDocument { get; set; }
    public int MaxErrors { get; set; }
    public bool ProcessInlineSchema { get; set; }
}
```

| Property | Kind | Default Value | Description | Showcase Level |
|---|---|---|---|---|
| `IncludeWarnings` | `bool` | `false` | Indicates whether non-fatal warnings are accumulated with the `[Warning]` prefix. If `TreatWarningsAsErrors` is `true`, this property always returns `true`. | Levels 2, 6 |
| `TreatWarningsAsErrors` | `bool` | `false` | When `true`, any schema warning causes validation failure (`result.IsFailure == true`). | Levels 2, 6 |
| `MaxCharactersInDocument` | `long` | `10_000_000` | Maximum character limit permitted in the document to mitigate DoS attacks. `0` = unlimited. Requires `>= 0`. | Levels 2, 6 |
| `MaxErrors` | `int` | `100` | Maximum number of errors and warnings collected before truncating to prevent memory exhaustion. Requires `> 0`. | Levels 2, 6 |
| `ProcessInlineSchema` | `bool` | `false` | Indicates whether inline schemas encountered in the XML document are processed. Defaults to `false` to prevent schema poisoning attacks. | Level 2 |

---

### 2.7 [`XmlValidationServiceCollectionExtensions`](../src/EricksonLopez.Xml.Validation/XmlValidationServiceCollectionExtensions.cs)

```csharp
namespace EricksonLopez.Xml.Validation;

public static class XmlValidationServiceCollectionExtensions
{
    public static IServiceCollection AddXmlValidation(this IServiceCollection services);
    public static IServiceCollection AddXmlValidation(this IServiceCollection services, Action<XmlValidationOptions>? configure);
}
```

| Member | Kind | Signature | Return | DI Registration Behavior | Showcase Level |
|---|---|---|---|---|---|
| `AddXmlValidation` | Extension Method | `(this IServiceCollection services)` | `IServiceCollection` | Registers `IXmlSchemaCache` as `XmlSchemaCache` (Singleton) and `IXmlSchemaValidator` as `XmlSchemaValidator` (Singleton) with default options. | Levels 2, 8 |
| `AddXmlValidation` | Extension Method | `(this IServiceCollection services, Action<XmlValidationOptions>? configure)` | `IServiceCollection` | Registers singletons applying the options configuration delegate and resolving `ILogger<XmlSchemaValidator>` automatically. | Level 2 |

---

## 3. Canonical Error Taxonomy (`EricksonLopez.Result`)

| Error Code | Error Type (`Error.Type`) | Triggering Condition | Message Format | Showcase Level |
|---|---|---|---|---|
| `XmlValidation.SchemaNotRegistered` | `NotFound` | Validation requested against a `targetNamespace` not registered in the cache. | `"No precompiled XSD schema registered for namespace '{targetNamespace}'."` | Levels 1, 6 |
| `XmlValidation.SchemaViolation` | `Validation` | The XML document violates one or more rules of the precompiled XSD schema. | `"Line {LineNumber}, Pos {LinePosition}: {Message} \| ..."` | Levels 1, 3, 6, 10 |
| `XmlValidation.XmlMalformed` | `Validation` | Syntactically invalid XML, prohibited DTD markup (XXE), or document size limit exceeded. | `"Malformed XML at Line {LineNumber}, Position {LinePosition}: {Message}"` | Levels 1, 6 |

---

## 4. Structured Diagnostic Events (`[LoggerMessage]`)

| EventId | LogLevel | Message Template | Showcase Level |
|:---:|:---:|---|---|
| `1001` | `Debug` | `"Schema not registered for namespace '{Namespace}'"` | Level 2 |
| `1002` | `Debug` | `"Validation failed for namespace '{Namespace}': {Errors}"` | Level 2 |
| `1003` | `Debug` | `"Validation succeeded for namespace '{Namespace}'"` | Level 2 |
| `1004` | `Debug` | `"Validation cancelled for namespace '{Namespace}'"` | Level 2 |
| `1005` | `Debug` | `"Malformed XML at Line {LineNumber}, Position {LinePosition} for namespace '{Namespace}': {XmlMessage}"` | Level 2 |
