# Level 02: Full Configuration & Dependency Injection

## Overview

In production microservices and enterprise applications, XML validation must integrate cleanly into the host dependency injection container, emit zero-allocation structured logs, and enforce strict security boundaries through configurable validation options.

---

## 1. Microsoft Dependency Injection Registration

`EricksonLopez.Xml.Validation` provides idiomatic extension methods for `IServiceCollection`. Registration installs both `IXmlSchemaCache` (`XmlSchemaCache`) and `IXmlSchemaValidator` (`XmlSchemaValidator`) with **Singleton** lifetime:

```csharp
using EricksonLopez.Xml.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

// Configure console logging to observe zero-allocation [LoggerMessage] events
services.AddLogging(builder =>
{
    builder.AddSimpleConsole(opts =>
    {
        opts.SingleLine = true;
        opts.TimestampFormat = "HH:mm:ss.fff ";
    });
    builder.SetMinimumLevel(LogLevel.Debug);
});

// Register validation engine with complete XmlValidationOptions surface
services.AddXmlValidation(options =>
{
    options.IncludeWarnings = true;
    options.TreatWarningsAsErrors = false;
    options.MaxCharactersInDocument = 5_000_000; // Defense against XML DoS
    options.MaxErrors = 50;                     // Cap error collection against memory exhaustion
    options.ProcessInlineSchema = false;        // Defense against inline schema poisoning
});

using var serviceProvider = services.BuildServiceProvider();
```

---

## 2. Complete Configuration Options Surface (`XmlValidationOptions`)

| Option | Type | Default | Description & Security Rationale |
|---|---|---|---|
| `IncludeWarnings` | `bool` | `false` | When `true`, non-fatal XSD schema warnings are captured and formatted with the `[Warning]` prefix. Automatically returns `true` if `TreatWarningsAsErrors` is enabled. |
| `TreatWarningsAsErrors` | `bool` | `false` | When `true`, any XSD schema warning causes validation to fail (`result.IsFailure == true`). Critical for strict financial (ISO 20022) and electronic invoicing (UBL 2.1) compliance. |
| `MaxCharactersInDocument` | `long` | `10_000_000` | Maximum number of characters allowed in the XML document to defend against denial-of-service (DoS) payload bombs. Set to `0` for unlimited. |
| `MaxErrors` | `int` | `100` | Maximum validation errors and warnings collected before truncating further additions, preventing memory exhaustion under adversarial payloads. |
| `ProcessInlineSchema` | `bool` | `false` | When `false` (default), inline schemas are ignored to prevent schema poisoning attacks on untrusted input. |

---

## 3. Singleton Lifecycle Guarantee Across Scopes

Both `IXmlSchemaCache` and `IXmlSchemaValidator` are registered as singletons. Creating multiple dependency injection scopes (`CreateScope()`) shares the exact same instance, ensuring that schema compilation is never repeated across HTTP requests:

```csharp
using var scope1 = serviceProvider.CreateScope();
using var scope2 = serviceProvider.CreateScope();

var cache1 = scope1.ServiceProvider.GetRequiredService<IXmlSchemaCache>();
var cache2 = scope2.ServiceProvider.GetRequiredService<IXmlSchemaCache>();

bool sameInstance = ReferenceEquals(cache1, cache2); // True
```

---

## 4. Compile-Time Diagnostic Logging (`[LoggerMessage]`)

`XmlSchemaValidator` incorporates compile-time partial methods powered by the .NET `[LoggerMessage]` source generator, guaranteeing zero heap allocations during logging:

| Event ID | Level | Event Name | Description |
|:---:|:---:|---|---|
| `1001` | `Debug` | `LogSchemaNotRegistered` | Emitted when validation targets an unregistered namespace. |
| `1002` | `Debug` | `LogValidationFailed` | Emitted when document fails XSD constraints or contains warnings under strict policy. |
| `1003` | `Debug` | `LogValidationSucceeded` | Emitted upon successful validation. |
| `1004` | `Debug` | `LogValidationCancelled` | Emitted when cooperative cancellation aborts async validation. |
| `1005` | `Debug` | `LogMalformedXml` | Emitted on XML syntax errors or prohibited DTD/XXE markup. |

---

## Code Reference
- [`Level02FullConfiguration.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level02FullConfiguration.cs)
