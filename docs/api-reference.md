# API Reference — EricksonLopez.Xml.Validation

Comprehensive technical reference in the style of Microsoft Learn for all public types, methods, overloads, properties, and configuration options in **`EricksonLopez.Xml.Validation`**.

---

## Namespace: `EricksonLopez.Xml.Validation`

---

## 1. `IXmlSchemaCache`

Defines the contract for a thread-safe, in-memory registry and cache for precompiled XSD schema sets (`XmlSchemaSet`).

```csharp
namespace EricksonLopez.Xml.Validation;

public interface IXmlSchemaCache
```

### Methods

---

#### `RegisterSchema(string, string)`
Compiles and registers an XSD schema definition provided as an XML string.

```csharp
void RegisterSchema(string targetNamespace, string xsdContent);
```

- **Parameters:**
  - `targetNamespace` (`string`): The target XML namespace of the schema. Cannot be `null`.
  - `xsdContent` (`string`): The complete XSD schema XML definition. Cannot be `null`.
- **Return Value:** `void`.
- **Exceptions:**
  - `ArgumentNullException`: Thrown if `targetNamespace` or `xsdContent` is `null`.
  - `XmlException`: Thrown if `xsdContent` is not well-formed XML.
  - `XmlSchemaException`: Thrown if the XSD contains schema structural or syntax compilation errors.
- **Remarks:** The schema is compiled immediately into an immutable `XmlSchemaSet` with Anti-XXE settings enforced (`DtdProcessing.Prohibit`, `XmlResolver = null`). If a schema is already registered for `targetNamespace`, it is atomically replaced in the cache.
- **Basic Example:**
  ```csharp
  cache.RegisterSchema("https://ericksonlopez.dev/schemas/orders", xsdContentString);
  ```
- **Advanced Example:**
  ```csharp
  // Dynamically reading schema from an embedded manifest resource stream
  using var stream = assembly.GetManifestResourceStream("MyApp.Schemas.order.xsd")!;
  using var reader = new StreamReader(stream, Encoding.UTF8);
  string xsd = reader.ReadToEnd();
  cache.RegisterSchema("https://ericksonlopez.dev/schemas/orders", xsd);
  ```
- **Best Practices:** Register schemas during host startup or DI bootstrapping to allow subsequent validation requests to benefit from $O(1)$ lock-free lookups.
- **Performance:** Schema compilation is CPU-intensive. Avoid invoking this method repeatedly on request hot paths.
- **Common Mistakes:** Invoking `RegisterSchema` inside a web request handler rather than during application initialization.
- **When to Use:** When XSD content originates from embedded assembly resources, database records, or in-memory strings.
- **When NOT to Use:** When the schema resides as a physical file on disk (use `RegisterSchemaFile` instead).

---

#### `RegisterSchema(string, Stream)`
Compiles and registers an XSD schema provided as a readable data stream.

```csharp
void RegisterSchema(string targetNamespace, Stream xsdStream);
```

- **Parameters:**
  - `targetNamespace` (`string`): The target XML namespace. Cannot be `null`.
  - `xsdStream` (`Stream`): Readable stream containing the XSD schema data. Cannot be `null`.
- **Return Value:** `void`.
- **Exceptions:**
  - `ArgumentNullException`: Thrown if `targetNamespace` or `xsdStream` is `null`.
  - `XmlException`: Thrown if the stream content is not well-formed XML.
  - `XmlSchemaException`: Thrown if the stream contains schema structural or syntax errors.
- **Remarks:** Parses the stream using an `XmlReader` configured with `DtdProcessing.Prohibit` and `XmlResolver = null`.
- **Basic Example:**
  ```csharp
  using var stream = File.OpenRead("schemas/invoice.xsd");
  cache.RegisterSchema("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2", stream);
  ```
- **Advanced Example:**
  ```csharp
  // Direct registration from an HTTP download stream without intermediate string allocation
  using var httpClient = new HttpClient();
  using var responseStream = await httpClient.GetStreamAsync("https://cdn.example.com/schemas/orders.xsd");
  cache.RegisterSchema("https://example.com/orders", responseStream);
  ```
- **Best Practices:** Ensure the input stream position is reset to `0` before passing it if the stream has been previously read.
- **Performance:** Avoids allocating large intermediate string buffers on the Gen 0 heap.
- **Common Mistakes:** Passing a closed or disposed stream.
- **When to Use:** Reading schema files directly from `FileStream`, network streams, or memory streams without allocating intermediate strings.
- **When NOT to Use:** When the schema is already available as a string literal in code.

---

#### `RegisterSchema(string, ReadOnlySpan<byte>)`
Compiles and registers an XSD schema from a contiguous UTF-8 encoded byte span.

```csharp
void RegisterSchema(string targetNamespace, ReadOnlySpan<byte> utf8Xsd);
```

- **Parameters:**
  - `targetNamespace` (`string`): The target XML namespace. Cannot be `null`.
  - `utf8Xsd` (`ReadOnlySpan<byte>`): Memory buffer containing UTF-8 bytes of the schema.
- **Return Value:** `void`.
- **Exceptions:**
  - `ArgumentNullException`: Thrown if `targetNamespace` is `null`.
  - `XmlException`: Thrown if the byte sequence is not well-formed XML.
  - `XmlSchemaException`: Thrown if compilation errors are detected.
- **Remarks:** Uses unmanaged pointer pinning (`fixed`) over the span to create an `UnmanagedMemoryStream`, avoiding heap array copies.
- **Basic Example:**
  ```csharp
  byte[] xsdBytes = Encoding.UTF8.GetBytes(orderXsdString);
  cache.RegisterSchema("https://ericksonlopez.dev/schemas/orders", xsdBytes.AsSpan());
  ```
- **Advanced Example:**
  ```csharp
  // Read schema bytes directly into span
  byte[] schemaBytes = File.ReadAllBytes("schema.xsd");
  ReadOnlySpan<byte> schemaSpan = schemaBytes.AsSpan();
  cache.RegisterSchema("https://example.com/schema", schemaSpan);
  ```
- **Best Practices:** Ideal when schemas are loaded from memory-mapped files or native buffers.
- **Performance:** Minimizes Garbage Collection overhead during system boot.
- **Common Mistakes:** Passing an empty span, which causes an `XmlException` from the underlying reader.
- **When to Use:** High-throughput or resource-constrained startup environments where byte buffers are already available.
- **When NOT to Use:** When reading schemas from standard filesystem paths (use `RegisterSchemaFile`).

---

#### `ContainsSchema(string)`
Determines whether a precompiled schema is registered for the specified target namespace.

```csharp
bool ContainsSchema(string targetNamespace);
```

- **Parameters:**
  - `targetNamespace` (`string`): The target XML namespace to check. Cannot be `null`.
- **Return Value:** `bool`: `true` if a schema for `targetNamespace` is registered; otherwise, `false`.
- **Exceptions:** `ArgumentNullException`: Thrown if `targetNamespace` is `null`.
- **Performance:** $O(1)$ lock-free read on the underlying `ConcurrentDictionary`.
- **When to Use:** Checking schema registration status before invoking validation in dynamic multi-tenant workflows.
- **When NOT to Use:** Calling before every single `Validate()` call (the validator checks the cache internally with zero overhead).

---

#### `IsRootElementDeclared(string, string, string)`
Determines whether the specified root element qualified name is declared as a global element in the registered schema.

```csharp
bool IsRootElementDeclared(string targetNamespace, string localName, string namespaceUri);
```

- **Parameters:**
  - `targetNamespace` (`string`): The target XML namespace of the schema. Cannot be `null`.
  - `localName` (`string`): The local name of the root element. Cannot be `null`.
  - `namespaceUri` (`string`): The namespace URI of the root element. Cannot be `null`.
- **Return Value:** `bool`: `true` if the element is declared as a global element in the target schema; otherwise, `false`.
- **Exceptions:** `ArgumentNullException`: Thrown if `targetNamespace`, `localName`, or `namespaceUri` is `null`.
- **Remarks:** Populated during schema registration by inspecting `schemaSet.GlobalElements`. Allows validating that an incoming XML document specifies an expected top-level root element.
- **Basic Example:**
  ```csharp
  bool isDeclared = cache.IsRootElementDeclared("https://example.com/orders", "Order", "https://example.com/orders");
  ```
- **Advanced Example:**
  ```csharp
  // Pre-validation root element guard in an API gateway
  if (!cache.IsRootElementDeclared(expectedNamespace, rootLocalName, rootNsUri))
  {
      return Results.BadRequest($"Root element '{rootLocalName}' is not a recognized document root.");
  }
  ```
- **Best Practices:** Use to fast-fail documents with unexpected root nodes before full subtree validation.
- **Performance:** $O(1)$ hash set lookup.
- **Common Mistakes:** Passing the prefix instead of the `localName`.
- **When to Use:** Routing incoming payloads or enforcing strict root element compliance.
- **When NOT to Use:** When document root element variation is handled entirely through XSD choice groups.

---

#### `Clear()`
Removes all registered schemas from the cache.

```csharp
void Clear();
```

- **Return Value:** `void`.
- **Remarks:** This operation is thread-safe. Concurrent registrations or lookups in progress complete normally; subsequent operations see an empty cache.
- **When to Use:** Test teardown, schema reloading in development environments, or tenant cache resets.
- **When NOT to Use:** Production hot paths during active traffic.

---

### Properties

#### `Count`
Gets the total number of precompiled schema sets currently registered in the cache.

```csharp
int Count { get; }
```

- **Return Value:** `int`: The number of registered namespaces.
- **Performance:** $O(1)$ read.

---

## 2. `XmlSchemaCache`

Thread-safe, sealed implementation of `IXmlSchemaCache` backed by `ConcurrentDictionary<string, SchemaCacheEntry>` with Anti-XXE defaults enforced on all compilation paths.

```csharp
public sealed class XmlSchemaCache : IXmlSchemaCache
```

### Constructors

#### `XmlSchemaCache()`
Initializes a new empty instance of `XmlSchemaCache`.

```csharp
public XmlSchemaCache();
```

- **Remarks:** Initializes the internal dictionary using `StringComparer.Ordinal` for exact namespace matching.

---

## 3. `XmlSchemaCacheExtensions`

Provides extension methods for `IXmlSchemaCache` to support schema registration from the filesystem.

```csharp
namespace EricksonLopez.Xml.Validation;

public static class XmlSchemaCacheExtensions
```

### Methods

---

#### `RegisterSchemaFile(this IXmlSchemaCache, string, string)`
Reads an XSD file from disk, compiles it, and registers it in the cache.

```csharp
public static void RegisterSchemaFile(this IXmlSchemaCache cache, string targetNamespace, string filePath);
```

- **Parameters:**
  - `cache` (`IXmlSchemaCache`): The target schema cache.
  - `targetNamespace` (`string`): The target XML namespace.
  - `filePath` (`string`): Absolute or relative path to the `.xsd` file.
- **Exceptions:**
  - `ArgumentNullException`: Any argument is `null`.
  - `FileNotFoundException`: The file does not exist at `filePath`.
  - `XmlException`: The file does not contain well-formed XML.
  - `XmlSchemaException`: The schema contains compilation errors.
- **Basic Example:**
  ```csharp
  cache.RegisterSchemaFile("https://ericksonlopez.dev/schemas/orders", "Schemas/orders.xsd");
  ```
- **When to Use:** Server bootstrap when individual schema files reside in known directory paths.
- **When NOT to Use:** When schemas are embedded as assembly resources or stored in databases.

---

#### `RegisterSchemasFromDirectory(this IXmlSchemaCache, string, string)`
Recursively scans a directory on disk, compiling and registering all matching XSD files.

```csharp
public static int RegisterSchemasFromDirectory(this IXmlSchemaCache cache, string directoryPath, string searchPattern = "*.xsd");
```

- **Parameters:**
  - `cache` (`IXmlSchemaCache`): The target schema cache.
  - `directoryPath` (`string`): Directory path containing `.xsd` files.
  - `searchPattern` (`string`): File matching pattern (defaults to `"*.xsd"`).
- **Return Value:** `int`: The total number of schemas successfully compiled and registered.
- **Exceptions:**
  - `ArgumentNullException`: Any argument is `null`.
  - `DirectoryNotFoundException`: `directoryPath` does not exist.
  - `XmlSchemaException`: Schema compilation errors occur.
- **Remarks:** Groups schemas by `targetNamespace` and compiles them with Anti-XXE settings. If multiple files share the same `targetNamespace`, they are compiled together into a unified `XmlSchemaSet`.
- **Basic Example:**
  ```csharp
  int loaded = cache.RegisterSchemasFromDirectory("C:/app/schemas");
  ```
- **When to Use:** Bulk preloading enterprise schema catalogs (e.g., UBL 2.1, ISO 20022) at application startup.
- **When NOT to Use:** Dynamic run-time loading of single schemas during web request processing.

---

## 4. `IXmlSchemaValidator`

Defines the contract for validating XML documents against precompiled schema sets managed in `IXmlSchemaCache`.

```csharp
namespace EricksonLopez.Xml.Validation;

public interface IXmlSchemaValidator
```

### Methods

---

#### `Validate(string, string)`
Validates an XML document string against the registered schema for the specified namespace.

```csharp
Result<bool> Validate(string xml, string targetNamespace);
```

- **Parameters:**
  - `xml` (`string`): The raw XML document content. Cannot be `null`.
  - `targetNamespace` (`string`): Target XML namespace of the precompiled schema. Cannot be `null`.
- **Return Value:** `Result<bool>`.
  - Success: `Result<bool>.Success(true)`.
  - Failure: `Result<bool>.Failure(Error)` where `Error.Code` is one of:
    - `"XmlValidation.SchemaNotRegistered"` (`Error.NotFound`)
    - `"XmlValidation.SchemaViolation"` (`Error.Validation`)
    - `"XmlValidation.XmlMalformed"` (`Error.Validation`)
- **Exceptions:**
  - `ArgumentNullException`: Thrown if `xml` or `targetNamespace` is `null`.
  - `InvalidOperationException`: Thrown if the configured `IXmlSchemaCache` does not implement the schema set provider required by the validator engine.
- **Basic Example:**
  ```csharp
  var result = validator.Validate(xmlString, "https://example.com/orders");
  if (result.IsSuccess) { ... }
  ```
- **When to Use:** Small to medium XML documents already materialized in memory.
- **When NOT to Use:** Multi-megabyte XML documents; use `Stream` or `ReadOnlySpan<byte>` overloads to prevent Large Object Heap (LOH) fragmentation.

---

#### `Validate(Stream, string)`
Synchronously validates an XML data stream against the registered schema.

```csharp
Result<bool> Validate(Stream xmlStream, string targetNamespace);
```

- **Parameters:**
  - `xmlStream` (`Stream`): Readable stream containing the XML document. Cannot be `null`.
  - `targetNamespace` (`string`): Target XML namespace. Cannot be `null`.
- **Return Value:** `Result<bool>`.
- **Exceptions:**
  - `ArgumentNullException`: Thrown if `xmlStream` or `targetNamespace` is `null`.
  - `InvalidOperationException`: Thrown if the configured `IXmlSchemaCache` does not implement the schema set provider required by the validator engine.
- **Remarks:** Leaves the input stream open (`CloseInput = false`), allowing downstream consumers to rewind and deserialize the payload.
- **When to Use:** Synchronous I/O pipelines, file validation, or in-memory streams.
- **When NOT to Use:** ASP.NET Core request bodies on Kestrel worker threads (use `ValidateAsync` instead).

---

#### `Validate(ReadOnlySpan<byte>, string)`
Validates a contiguous UTF-8 encoded XML byte sequence against the registered schema.

```csharp
Result<bool> Validate(ReadOnlySpan<byte> utf8Xml, string targetNamespace);
```

- **Parameters:**
  - `utf8Xml` (`ReadOnlySpan<byte>`): Contiguous UTF-8 encoded byte sequence representing the XML document.
  - `targetNamespace` (`string`): Target XML namespace. Cannot be `null`.
- **Return Value:** `Result<bool>`.
- **Exceptions:**
  - `ArgumentNullException`: Thrown if `targetNamespace` is `null`.
  - `InvalidOperationException`: Thrown if the configured `IXmlSchemaCache` does not implement the schema set provider required by the validator engine.
- **Remarks:** An empty span returns `Error.Validation("XmlValidation.XmlMalformed", "Malformed XML: XML document payload is empty.")`. Validates via pinned pointer without intermediate string allocations.
- **Performance:** Highest throughput synchronous validation modality.
- **When to Use:** Network socket buffers, Kafka message bytes, or memory-mapped payloads.
- **When NOT to Use:** When the payload is already materialized as a .NET `string`.

---

#### `ValidateAsync(Stream, string, CancellationToken)`
Asynchronously validates an XML data stream against the registered schema with cooperative cancellation.

```csharp
Task<Result<bool>> ValidateAsync(
    Stream xmlStream,
    string targetNamespace,
    CancellationToken cancellationToken = default);
```

- **Parameters:**
  - `xmlStream` (`Stream`): Readable stream containing XML data. Cannot be `null`.
  - `targetNamespace` (`string`): Target XML namespace. Cannot be `null`.
  - `cancellationToken` (`CancellationToken`): Token to observe for cancellation requests.
- **Return Value:** `Task<Result<bool>>`.
- **Exceptions:**
  - `ArgumentNullException`: Thrown if `xmlStream` or `targetNamespace` is `null`.
  - `OperationCanceledException`: Thrown if cancellation is triggered before or during streaming validation.
  - `InvalidOperationException`: Thrown if the configured `IXmlSchemaCache` does not implement the schema set provider required by the validator engine.
- **Basic Example:**
  ```csharp
  var result = await validator.ValidateAsync(httpRequest.Body, "https://example.com/orders", ct);
  ```
- **When to Use:** Asynchronous network streams, such as `HttpRequest.Body` in ASP.NET Core Minimal APIs or controllers.
- **When NOT to Use:** In-memory byte buffers where synchronous span validation is available.

---

## 5. `XmlSchemaValidator`

Production implementation of `IXmlSchemaValidator` with Anti-XXE enforcement, structured diagnostic logging, and configurable options.

```csharp
public sealed partial class XmlSchemaValidator : IXmlSchemaValidator
```

### Constructors

#### `XmlSchemaValidator(IXmlSchemaCache)`
Initializes an instance with default `XmlValidationOptions` and `NullLogger`.

```csharp
public XmlSchemaValidator(IXmlSchemaCache schemaCache);
```

- **Remarks:** The supplied `schemaCache` must implement the library's internal schema set provider (such as `XmlSchemaCache`). If an incompatible third-party implementation of `IXmlSchemaCache` is supplied, `Validate` will throw `InvalidOperationException`.

#### `XmlSchemaValidator(IXmlSchemaCache, XmlValidationOptions, ILogger<XmlSchemaValidator>?)`
Initializes an instance with custom validation options and structured logging.

```csharp
public XmlSchemaValidator(
    IXmlSchemaCache schemaCache,
    XmlValidationOptions options,
    ILogger<XmlSchemaValidator>? logger = null);
```

---

## 6. `XmlValidationOptions`

Specifies configuration options controlling validation behavior, DoS defense limits, and warning severity.

```csharp
namespace EricksonLopez.Xml.Validation;

public sealed class XmlValidationOptions
```

### Properties

#### `IncludeWarnings`
Gets or sets a value indicating whether non-fatal XSD schema warnings are included in validation results.

```csharp
public bool IncludeWarnings { get; set; }
```
- **Default:** `false`.
- **Remarks:** When `true`, warnings are captured and appended with the prefix `[Warning]`. If `TreatWarningsAsErrors` is `true`, this property automatically returns `true`.

#### `TreatWarningsAsErrors`
Gets or sets a value indicating whether XSD schema warnings are treated as validation errors.

```csharp
public bool TreatWarningsAsErrors { get; set; }
```
- **Default:** `false`.
- **Remarks:** Setting this to `true` causes validation to fail (`result.IsFailure == true`) if any warning is reported.

#### `MaxCharactersInDocument`
Gets or sets the maximum number of characters allowed in the XML document to defend against denial-of-service (DoS) attacks.

```csharp
public long MaxCharactersInDocument { get; set; }
```
- **Default:** `10_000_000` characters (~10MB–20MB). Set to `0` for unlimited.
- **Exceptions:** `ArgumentOutOfRangeException`: If value is `< 0`.
- **Remarks:** Passed directly to `XmlReaderSettings.MaxCharactersInDocument`.

#### `MaxErrors`
Gets or sets the maximum number of validation errors and warnings collected before truncating further additions, preventing memory exhaustion.

```csharp
public int MaxErrors { get; set; }
```
- **Default:** `100`.
- **Exceptions:** `ArgumentOutOfRangeException`: If value is `<= 0`.

#### `ProcessInlineSchema`
Gets or sets a value indicating whether inline schemas encountered in the XML document are processed.

```csharp
public bool ProcessInlineSchema { get; set; }
```
- **Default:** `false`.
- **Remarks:** Kept `false` by default to defend against inline schema poisoning on untrusted input.

---

## 7. `XmlValidationServiceCollectionExtensions`

Dependency injection extension methods for `Microsoft.Extensions.DependencyInjection.IServiceCollection`.

```csharp
namespace EricksonLopez.Xml.Validation;

public static class XmlValidationServiceCollectionExtensions
{
    public static IServiceCollection AddXmlValidation(this IServiceCollection services);
    public static IServiceCollection AddXmlValidation(this IServiceCollection services, Action<XmlValidationOptions>? configure);
}
```

- `AddXmlValidation(services)`: Registers `IXmlSchemaCache` (`XmlSchemaCache`) and `IXmlSchemaValidator` (`XmlSchemaValidator`) as `Singleton` with default options.
- `AddXmlValidation(services, configure)`: Registers singletons with a custom options configuration delegate, automatically injecting `ILogger<XmlSchemaValidator>` if logging is registered in the container.
