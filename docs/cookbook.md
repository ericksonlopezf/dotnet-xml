# Cookbook — EricksonLopez.Xml.Validation

A collection of 10 production-tested recipes for solving real-world XML schema validation, security, and integration challenges in .NET enterprise architectures, built 100% on the public API of `EricksonLopez.Xml.Validation`.

---

## Table of Contents

- [Recipe 1: ASP.NET Core Setup with DI and Structured Logging](#recipe-1-aspnet-core-setup-with-di-and-structured-logging)
- [Recipe 2: Asynchronous HTTP Stream Validation in Endpoints](#recipe-2-asynchronous-http-stream-validation-in-endpoints)
- [Recipe 3: Bulk Preloading a Schema Directory at Host Startup](#recipe-3-bulk-preloading-a-schema-directory-at-host-startup)
- [Recipe 4: Reduced-Allocation Validation with ReadOnlySpan&lt;byte&gt;](#recipe-4-reduced-allocation-validation-with-readonlyspanbyte)
- [Recipe 5: Strict XSD Warning Enforcement for Regulatory Compliance](#recipe-5-strict-xsd-warning-enforcement-for-regulatory-compliance)
- [Recipe 6: Neutralizing XXE Injections and XML Entity Expansion Attacks](#recipe-6-neutralizing-xxe-injections-and-xml-entity-expansion-attacks)
- [Recipe 7: Multi-Tenant Schema Handling & Graceful Error Mapping](#recipe-7-multi-tenant-schema-handling--graceful-error-mapping)
- [Recipe 8: Decorator Pattern for Metrics and Telemetry Auditing](#recipe-8-decorator-pattern-for-metrics-and-telemetry-auditing)
- [Recipe 9: Safe Cancellation with CancellationToken on Large Payloads](#recipe-9-safe-cancellation-with-cancellationtoken-on-large-payloads)
- [Recipe 10: Railway-Oriented Programming with Result&lt;T&gt; in Endpoints](#recipe-10-railway-oriented-programming-with-resultt-in-endpoints)

---

## Recipe 1: ASP.NET Core Setup with DI and Structured Logging

### Problem
Integrate XSD schema validation into an ASP.NET Core application, registering components as singletons and enabling diagnostic logging without performance degradation.

### Solution
Use `services.AddXmlValidation()` in `Program.cs`.

### Complete Code
```csharp
using EricksonLopez.Xml.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// Register IXmlSchemaCache and IXmlSchemaValidator as Singletons
builder.Services.AddXmlValidation(options =>
{
    options.IncludeWarnings = true;
    options.TreatWarningsAsErrors = false;
});

var app = builder.Build();

// Preload schemas into singleton cache
var cache = app.Services.GetRequiredService<IXmlSchemaCache>();
cache.RegisterSchema("https://example.com/orders", "<xs:schema ...>...</xs:schema>");

app.Run();
```

### Explanation
`AddXmlValidation()` registers `IXmlSchemaCache` (`XmlSchemaCache`) and `IXmlSchemaValidator` (`XmlSchemaValidator`) in the container. The validator automatically resolves `ILogger<XmlSchemaValidator>` to emit zero-allocation `[LoggerMessage]` events.

---

## Recipe 2: Asynchronous HTTP Stream Validation in Endpoints

### Problem
Validate an incoming XML payload directly from `HttpRequest.Body` without loading the full document into memory as a `string`.

### Solution
Invoke `IXmlSchemaValidator.ValidateAsync(stream, targetNamespace, cancellationToken)`.

### Complete Code
```csharp
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;

public class XmlIngestionHandler
{
    private readonly IXmlSchemaValidator _validator;

    public XmlIngestionHandler(IXmlSchemaValidator validator)
    {
        _validator = validator;
    }

    public async Task<Result<bool>> HandleUploadAsync(Stream httpBodyStream, string schemaNamespace, CancellationToken ct)
    {
        // Direct stream validation
        var validationResult = await _validator.ValidateAsync(httpBodyStream, schemaNamespace, ct);

        if (validationResult.IsFailure)
        {
            // Returns structured error with exact line and position coordinates
            return validationResult;
        }

        return Result<bool>.Success(true);
    }
}
```

### Explanation
`ValidateAsync` parses nodes directly from the incoming network stream using `XmlReader.ReadAsync()`, keeping the application's memory footprint flat regardless of document size.

---

## Recipe 3: Bulk Preloading a Schema Directory at Host Startup

### Problem
Your service uses multiple XSD schema files distributed on disk and requires compiling and registering all of them during system bootstrap.

### Solution
Use `RegisterSchemasFromDirectory(directoryPath, searchPattern)`.

### Complete Code
```csharp
using System;
using System.IO;
using EricksonLopez.Xml.Validation;

public static class SchemaBootstrapper
{
    public static void Initialize(IXmlSchemaCache cache, string schemasDirectory)
    {
        if (!Directory.Exists(schemasDirectory))
        {
            throw new DirectoryNotFoundException($"Schema directory not found: {schemasDirectory}");
        }

        int registered = cache.RegisterSchemasFromDirectory(schemasDirectory, "*.xsd");
        Console.WriteLine($"[Startup] Successfully compiled and registered {registered} XSD schemas.");
    }
}
```

---

## Recipe 4: Reduced-Allocation Validation with ReadOnlySpan<byte>

### Problem
Validate XML payloads from binary network sockets, message brokers, or buffer pools (`ArrayPool<byte>`) without allocating intermediate `string` instances on the heap. This overload pins the UTF-8 buffer via `fixed` pointer and mounts an `UnmanagedMemoryStream` with zero payload array copying, completely avoiding UTF-16 `string` heap allocations.

### Solution
Use the `Validate(ReadOnlySpan<byte> utf8Xml, string targetNamespace)` overload.

### Complete Code
```csharp
using System;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;

public class HighThroughputProcessor
{
    private readonly IXmlSchemaValidator _validator;

    public HighThroughputProcessor(IXmlSchemaValidator validator)
    {
        _validator = validator;
    }

    public Result<bool> ProcessBuffer(byte[] buffer, int length, string targetNamespace)
    {
        ReadOnlySpan<byte> utf8XmlSpan = new ReadOnlySpan<byte>(buffer, 0, length);

        // Validation over contiguous memory
        return _validator.Validate(utf8XmlSpan, targetNamespace);
    }
}
```

---

## Recipe 5: Strict XSD Warning Enforcement for Regulatory Compliance

### Problem
In strict government or financial integrations (e.g., UBL 2.1 e-Invoices or ISO 20022), any schema warning must trigger immediate document rejection.

### Solution
Configure `IncludeWarnings = true` and `TreatWarningsAsErrors = true` via `XmlValidationOptions`.

### Complete Code
```csharp
using EricksonLopez.Xml.Validation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddXmlValidation(options =>
{
    options.IncludeWarnings = true;
    options.TreatWarningsAsErrors = true;
    options.MaxCharactersInDocument = 10_000_000; // Defense against XML DoS
    options.MaxErrors = 50;                     // Cap error collection against memory exhaustion
    options.ProcessInlineSchema = false;        // Defense against inline schema poisoning
});

var sp = services.BuildServiceProvider();
var validator = sp.GetRequiredService<IXmlSchemaValidator>();
```

---

## Recipe 6: Neutralizing XXE Injections and XML Entity Expansion Attacks

### Problem
Protect application infrastructure against XML entity expansion ("Billion Laughs") and local file exfiltration through DTD declarations.

### Solution
`EricksonLopez.Xml.Validation` enforces Anti-XXE by design; any incoming DTD markup is immediately rejected as `XmlValidation.XmlMalformed`.

### Complete Code
```csharp
using System;
using EricksonLopez.Xml.Validation;

var cache = new XmlSchemaCache();
cache.RegisterSchema("https://example.com/schema", "<xs:schema ...>...</xs:schema>");
var validator = new XmlSchemaValidator(cache);

// Malicious payload with external entity DTD
string maliciousXml = """
    <?xml version="1.0"?>
    <!DOCTYPE foo [<!ENTITY xxe SYSTEM "file:///etc/shadow">]>
    <foo xmlns="https://example.com/schema">&xxe;</foo>
    """;

var result = validator.Validate(maliciousXml, "https://example.com/schema");

if (result.IsFailure && result.Error.Code == "XmlValidation.XmlMalformed")
{
    Console.WriteLine("XXE injection attempt immediately blocked by DtdProcessing.Prohibit policy.");
}
```

---

## Recipe 7: Multi-Tenant Schema Handling & Graceful Error Mapping

### Problem
In a multi-tenant SaaS architecture where different clients submit documents with different schema versions, a request may arrive with an unsupported or unknown XML namespace.

### Solution
Map `XmlValidation.SchemaNotRegistered` to a controlled HTTP 404/400 response.

### Complete Code
```csharp
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;

public class TenantXmlService
{
    private readonly IXmlSchemaValidator _validator;

    public TenantXmlService(IXmlSchemaValidator validator)
    {
        _validator = validator;
    }

    public string ProcessDocument(string xml, string targetNamespace)
    {
        var result = _validator.Validate(xml, targetNamespace);

        if (result.IsFailure)
        {
            if (result.Error.Code == "XmlValidation.SchemaNotRegistered")
            {
                return $"The schema for namespace '{targetNamespace}' is not supported by this tenant.";
            }

            return $"Validation violation: {result.Error.Description}";
        }

        return "Document accepted.";
    }
}
```

---

## Recipe 8: Decorator Pattern for Metrics and Telemetry Auditing

### Problem
Collect execution latency metrics (e.g., for Prometheus or OpenTelemetry) without altering the internal code of `IXmlSchemaValidator`.

### Solution
Implement an `IXmlSchemaValidator` decorator.

### Complete Code
```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;

public sealed class MetricsXmlSchemaValidatorDecorator : IXmlSchemaValidator
{
    private readonly IXmlSchemaValidator _inner;

    public MetricsXmlSchemaValidatorDecorator(IXmlSchemaValidator inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public Result<bool> Validate(string xml, string targetNamespace)
    {
        var sw = Stopwatch.StartNew();
        var result = _inner.Validate(xml, targetNamespace);
        sw.Stop();
        RecordMetric(targetNamespace, sw.ElapsedMilliseconds, result.IsSuccess);
        return result;
    }

    public Result<bool> Validate(Stream xmlStream, string targetNamespace)
        => _inner.Validate(xmlStream, targetNamespace);

    public Result<bool> Validate(ReadOnlySpan<byte> utf8Xml, string targetNamespace)
        => _inner.Validate(utf8Xml, targetNamespace);

    public Task<Result<bool>> ValidateAsync(Stream xmlStream, string targetNamespace, CancellationToken cancellationToken = default)
        => _inner.ValidateAsync(xmlStream, targetNamespace, cancellationToken);

    private static void RecordMetric(string ns, long elapsedMs, bool success)
    {
        Console.WriteLine($"[Metric] Namespace: {ns} | Duration: {elapsedMs}ms | Success: {success}");
    }
}
```

---

## Recipe 9: Safe Cancellation with CancellationToken on Large Payloads

### Problem
Validate large XML files while ensuring that if a client disconnects or an HTTP timeout fires, parsing aborts immediately and releases underlying stream handles.

### Solution
Pass a `CancellationToken` to `ValidateAsync` and handle `OperationCanceledException`.

### Complete Code
```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Xml.Validation;

public static async Task ValidateWithTimeoutAsync(
    IXmlSchemaValidator validator,
    Stream largeXmlStream,
    string targetNamespace,
    TimeSpan timeout)
{
    using var cts = new CancellationTokenSource(timeout);

    try
    {
        var result = await validator.ValidateAsync(largeXmlStream, targetNamespace, cts.Token);
        Console.WriteLine($"Validation completed within timeout: {result.IsSuccess}");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Validation exceeded time limit and was safely aborted.");
    }
}
```

---

## Recipe 10: Railway-Oriented Programming with Result<T> in Endpoints

### Problem
Chain XML validation into a functional pipeline without nested `if (result.IsFailure)` blocks or exception throwing.

### Solution
Use the Railway-Oriented Programming model supported by `EricksonLopez.Result`.

### Complete Code
```csharp
using System;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;

public class OrderSubmissionService
{
    private readonly IXmlSchemaValidator _validator;

    public OrderSubmissionService(IXmlSchemaValidator validator)
    {
        _validator = validator;
    }

    public Result<string> SubmitOrder(string xmlPayload, string targetNamespace)
    {
        // Step 1: Validate against precompiled XSD schema
        var validationResult = _validator.Validate(xmlPayload, targetNamespace);

        if (validationResult.IsFailure)
        {
            return Result<string>.Failure(validationResult.Error);
        }

        // Step 2: Order execution (only reached if XML is valid and safe)
        string orderId = "ORD-" + Guid.NewGuid().ToString("N")[..8];
        return Result<string>.Success(orderId);
    }
}
```
