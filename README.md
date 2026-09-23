# EricksonLopez.Xml.Validation

High-performance, memory-efficient, enterprise-grade XSD schema validation engine and precompiled schema cache for modern .NET.

[![CI](https://img.shields.io/github/actions/workflow/status/ericksonlopezf/dotnet-xml/ci.yml?branch=main&style=for-the-badge&logo=githubactions&logoColor=white&label=CI)](https://github.com/ericksonlopezf/dotnet-xml/actions)
[![Coverage](https://img.shields.io/codecov/c/github/ericksonlopezf/dotnet-xml?style=for-the-badge&logo=codecov&logoColor=white)](https://codecov.io/gh/ericksonlopezf/dotnet-xml)
[![Quality Gate](https://img.shields.io/sonar/quality_gate/ericksonlopezf_dotnet-xml?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonarcloud&logoColor=white)](https://sonarcloud.io/summary/new_code?id=ericksonlopezf_dotnet-xml)
[![Mutation Score](https://img.shields.io/badge/Mutation_Score-100%25-brightgreen?style=for-the-badge&logo=stryker&logoColor=white)](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/testing-roadmap.md)
[![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Xml.Validation?style=for-the-badge&logo=nuget&logoColor=white&color=512BD4)](https://www.nuget.org/packages/EricksonLopez.Xml.Validation)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EricksonLopez.Xml.Validation?style=for-the-badge&logo=nuget&logoColor=white&color=004880)](https://www.nuget.org/packages/EricksonLopez.Xml.Validation)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://github.com/ericksonlopezf/dotnet-xml/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET_8_%7C_9_%7C_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![NativeAOT](https://img.shields.io/badge/NativeAOT-Compatible-brightgreen?style=for-the-badge)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot)

---

**EricksonLopez.Xml.Validation** is an enterprise-grade XSD schema validation engine and thread-safe precompiled schema cache for modern .NET (`.NET 8`, `.NET 9`, `.NET 10`). Architected with **Anti-XXE protection enforced by default**, **structured `Result<bool>` error reporting** (Railway-Oriented Programming via `EricksonLopez.Result`), and **first-class Microsoft Dependency Injection integration**, it eliminates the severe security hazards, CPU exception-handling overhead, and repetitive compilation bottlenecks of legacy BCL XML APIs. The engine features reduced-allocation streaming with zero intermediate managed string allocations via `ReadOnlySpan<byte>`, zero-allocation compile-time `[LoggerMessage]` diagnostics, and certified compatibility under **Native AOT** and trimming.

---

## Table of Contents

- [What Problem It Solves](#-what-problem-it-solves)
- [Key Features](#-key-features)
- [Ecosystem](#-ecosystem)
- [Documentation](#-documentation)
  - [Step-by-Step Interactive Showcase (Levels 00 to 10)](#-step-by-step-interactive-showcase-levels-00-to-10)
  - [Technical Reference & Architecture Guides](#-technical-reference--architecture-guides)
- [Installation](#-installation)
  - [1. Core Package (Required)](#1-core-package-required)
  - [2. Optional Framework & Integration Packages](#2-optional-framework--integration-packages)
  - [3. Testing & Assertion Packages](#3-testing--assertion-packages)
- [Quick Start](#-quick-start)
  - [1. Standalone Setup & Schema Registration](#1-standalone-setup--schema-registration)
  - [2. Validating Documents via Result Pattern](#2-validating-documents-via-result-pattern)
  - [3. Reduced-Allocation Validation with ReadOnlySpan&lt;byte&gt;](#3-reduced-allocation-validation-with-readonlyspanbyte)
  - [4. Error Pattern Matching & HTTP RFC 9457 Mapping](#4-error-pattern-matching--http-rfc-9457-mapping)
- [Core Use Cases](#-core-use-cases)
  - [Use Case 1: Perimeter Defensive Gateway in ASP.NET Core Minimal APIs](#use-case-1-perimeter-defensive-gateway-in-aspnet-core-minimal-apis)
  - [Use Case 2: Asynchronous HTTP Body Stream Validation with Cooperative Cancellation](#use-case-2-asynchronous-http-body-stream-validation-with-cooperative-cancellation)
  - [Use Case 3: High-Throughput In-Memory Queue Buffer Processing](#use-case-3-high-throughput-in-memory-queue-buffer-processing)
  - [Use Case 4: Bulk Schema Precompilation on Application Startup](#use-case-4-bulk-schema-precompilation-on-application-startup)
  - [Use Case 5: Strict Regulatory Compliance with Warning Escalation](#use-case-5-strict-regulatory-compliance-with-warning-escalation)
  - [Use Case 6: Custom Telemetry and Metrics Enrichment via Decorator Pattern](#use-case-6-custom-telemetry-and-metrics-enrichment-via-decorator-pattern)
- [Configuration & Integrations](#-configuration--integrations)
  - [Microsoft Dependency Injection Integration](#microsoft-dependency-injection-integration)
  - [Zero-Allocation Diagnostic Logging](#zero-allocation-diagnostic-logging)
  - [Native AOT & Trimming Compatibility](#native-aot--trimming-compatibility)
- [Testing & Quality](#-testing--quality)
  - [Authoritative Quality Gates Matrix](#authoritative-quality-gates-matrix)
  - [Testing Pyramid & Taxonomy](#testing-pyramid--taxonomy)
  - [Stryker.NET Mutation Testing](#strykernet-mutation-testing)
- [Performance Benchmarks](#-performance-benchmarks)
  - [Primary Operations Benchmark](#primary-operations-benchmark)
  - [Precompiled Cache vs Ad-Hoc BCL Compilation](#precompiled-cache-vs-ad-hoc-bcl-compilation)
- [Compatibility & Technical Matrix](#-compatibility--technical-matrix)
  - [Target Frameworks & Runtime Matrix](#target-frameworks--runtime-matrix)
  - [Target Framework & Lifecycle Policy](#target-framework--lifecycle-policy)
  - [Error Taxonomy & RFC 9457 Problem Details Mapping](#error-taxonomy--rfc-9457-problem-details-mapping)
- [Architecture & Design Principles](#-architecture--design-principles)
  - [Validation Decision Pipeline](#validation-decision-pipeline)
  - [Validation Lifecycle State Machine](#validation-lifecycle-state-machine)
  - [Defensive Perimeter Ingestion Pipeline](#defensive-perimeter-ingestion-pipeline)
- [Best Practices & Anti-Patterns](#-best-practices--anti-patterns)
  - [Recommended vs Avoid](#recommended-vs-avoid)
- [Troubleshooting & Common Pitfalls](#-troubleshooting--common-pitfalls)
  - [1. XmlValidation.SchemaNotRegistered](#1-xmlvalidationschemanotregistered)
  - [2. XmlValidation.XmlMalformed with DTD Detected](#2-xmlvalidationxmlmalformed-with-dtd-detected)
  - [3. Stream Closed or Consumed Before Validation](#3-stream-closed-or-consumed-before-validation)
  - [4. Schema Warnings Not Causing Validation Failure](#4-schema-warnings-not-causing-validation-failure)
- [Part of the EricksonLopez Ecosystem](#-part-of-the-ericksonlopez-ecosystem)
- [Contributing](#-contributing)
  - [Prerequisites](#prerequisites)
  - [Local Development Commands](#local-development-commands)
  - [Community Resources](#community-resources)
- [License](#-license)

---

## 🎯 What Problem It Solves

Validating XML against XML Schema Definitions (XSD) in modern enterprise systems introduces critical challenges across security, allocation pressure, latency, and reliability:

### 1. Critical Security Vulnerabilities (XXE & Billion Laughs)
The default configurations of traditional BCL XML classes (`XmlDocument`, legacy `XmlReaderSettings`, `XmlSchemaSet`) historically permit external DTD processing, resolving untrusted URIs, and expanding internal entities. Unsanitized XML payloads expose systems to **XML External Entity (XXE)** injection (OWASP Top 10, CWE-611), Server-Side Request Forgery (SSRF), local file disclosure (`file:///etc/passwd`), and denial-of-service via exponential entity expansion (Billion Laughs attack).

### 2. The Performance Penalty of Exception-Driven Control Flow
Standard BCL validation relies heavily on throwing `XmlSchemaValidationException` and `XmlException` whenever a document is malformed or invalid. In high-throughput ingestion pipelines, handling thousands of malformed or non-compliant payloads results in massive stack unrolling overhead, thread synchronization stalls, and excessive Garbage Collector pressure on Generation 0/1.

### 3. Repeated Schema Compilation Overhead
Compiling an XSD schema requires building a complex type graph, validating content models, resolving namespaces, and generating state machines. Ad-hoc compilation per request (`new XmlSchemaSet().Compile()`) wastes up to **47 ms per 1,000 requests** on small schemas and orders of magnitude more on massive schemas (e.g., UBL 2.1 e-Invoicing or ISO 20022 `pain.001`).

### 4. Excessive Heap Allocation and String Materialization
Standard XML validation workflows routinely require reading full payloads into `string` instances on the heap before validation can commence. For high-volume socket buffers, Kafka/RabbitMQ queue messages, or large payloads, this induces severe memory fragmentation and GC pauses.

### How `EricksonLopez.Xml.Validation` Solves This

- **Hardened Anti-XXE Defaults by Design:** Enforces `DtdProcessing.Prohibit` and `XmlResolver = null` across every internal reader and schema compilation pipeline. This protection cannot be bypassed through public APIs.
- **Railway-Oriented Functional Returns:** Replaces exception throws with structured `Result<bool>` returns powered by `EricksonLopez.Result`. Business code remains functional, predictable, and allocation-friendly.
- **$O(1)$ Precompiled Schema Cache:** The thread-safe `XmlSchemaCache` precompiles and stores schemas in a `ConcurrentDictionary<string, SchemaCacheEntry>` (keyed by target namespace, each entry holding a compiled `XmlSchemaSet` plus a pre-indexed global-element `HashSet`) with lock-free read paths, delivering **5x to 100x speedup** on repeated validations depending on schema complexity.
- **Reduced-Allocation Span Validation:** Full native support for `ReadOnlySpan<byte>` enables validating UTF-8 byte payloads via pinned unmanaged memory (`fixed` + `UnmanagedMemoryStream`), eliminating intermediate managed string allocations. BCL `XmlReader` internal buffers (~2.16 KB) still allocate on Gen 0.
- **Zero-Allocation Logging:** Structured diagnostic logging powered by compile-time source-generated `[LoggerMessage]` partial methods (Event IDs `1001`–`1005`).

---

## ⚡ Key Features

- 🛡️ **Anti-XXE Enforced by Default**: `DtdProcessing.Prohibit` and `XmlResolver = null` applied unconditionally. External entity resolution and DTD processing are prohibited on all execution paths.
- ⚡ **Precompiled Thread-Safe Schema Cache**: `ConcurrentDictionary`-backed `IXmlSchemaCache` indexed by `targetNamespace`, enabling thread-safe, lock-free read paths with $O(1)$ schema retrieval. Cache introspection via `ContainsSchema`, `Count`, `IsRootElementDeclared`, and `Clear` — see [api-inventory.md](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/api-inventory.md) for the full member catalog.
- 🚂 **Railway-Oriented Structured Errors**: Returns `Result<bool>` containing strongly typed error descriptors (`Error.Validation`, `Error.NotFound`) with zero control-flow exceptions.
- 📍 **Precise Line & Column Diagnostics**: Automatically extracts line numbers and line positions from `IXmlLineInfo` for actionable schema and syntax error diagnosis.
- 🌊 **Multi-Modal Input Support**: Direct validation over `string`, `Stream` (retaining stream ownership), `ReadOnlySpan<byte>` (no intermediate managed-string allocations; BCL `XmlReader` buffers ~2.16 KB on Gen 0), and asynchronous `Stream` with cooperative `CancellationToken`.
- 📂 **Bulk Directory Precompilation**: Recursively discovers, loads, and compiles all matching `.xsd` schemas from filesystem directories at startup via `RegisterSchemasFromDirectory()`.
- ⚠️ **Configurable Warning Escalation**: Granular control via `XmlValidationOptions` to capture non-fatal schema warnings or escalate them to full validation failures (`TreatWarningsAsErrors`).
- 📝 **Zero-Allocation Structured Diagnostics**: High-performance logging using C# compile-time `[LoggerMessage]` partial methods (Event IDs `1001`–`1005`).
- 🔌 **First-Class Microsoft DI Integration**: One-line container registration via `services.AddXmlValidation()` with idempotent singleton lifecycles (`TryAddSingleton`) and `IOptions<XmlValidationOptions>` support.
- 🚀 **100% Native AOT & Trim Safe**: Compiled with `<IsAotCompatible>true</IsAotCompatible>` and verified with dedicated ahead-of-time compilation smoke-test executables. Zero runtime reflection.
- 🎯 **Multi-Targeting Modern .NET**: Native support across `.NET 8.0 (LTS)`, `.NET 9.0 (STS)`, and `.NET 10.0`.

---

## 📦 Ecosystem

| Package | Version | Target Frameworks | Description |
|---|---|---|---|
| [`EricksonLopez.Xml.Validation`](https://www.nuget.org/packages/EricksonLopez.Xml.Validation) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Xml.Validation?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Xml.Validation) | `net8.0`, `net9.0`, `net10.0` | High-performance XSD schema validation engine with Anti-XXE defaults, precompiled cache, and Native AOT support. |

---

## 📚 Documentation

> 🌐 **Official Documentation:** [https://ericksonlopez.dev/xml](https://ericksonlopez.dev/xml) | **Technical Docs Hub:** [https://github.com/ericksonlopezf/dotnet-xml/tree/main/docs](https://github.com/ericksonlopezf/dotnet-xml/tree/main/docs)

### 🎓 Step-by-Step Interactive Showcase (Levels 00 to 10)

The repository includes an official, interactive reference implementation located in [samples/EricksonLopez.Xml.Showcase](https://github.com/ericksonlopezf/dotnet-xml/tree/main/samples/EricksonLopez.Xml.Showcase).

| Level | Topic | Description | Code Reference |
|---|---|---|---|
| [**Level 00**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-00-introduction.md) | **Conceptual Foundations & Anti-XXE Threat Modeling** | Core architectural philosophy, threat modeling, and comparison with raw BCL APIs | [`Level00Conceptual.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level00Conceptual.cs) |
| [**Level 01**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-01-basic-validation.md) | **Standalone Quick Start (No DI)** | Minimal standalone initialization, schema registration, and Result-based inspection | [`Level01QuickStart.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level01QuickStart.cs) |
| [**Level 02**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-02-full-configuration.md) | **Microsoft DI Configuration & Structured Logging** | Service registration, singleton lifecycles, options configuration, and structured logs | [`Level02FullConfiguration.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level02FullConfiguration.cs) |
| [**Level 03**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-03-real-world-use-cases.md) | **Real-World Enterprise Schemas** | Industrial validation using UBL 2.1 e-Invoice and ISO 20022 `pain.001` schemas | [`Level03RealWorldUseCases.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level03RealWorldUseCases.cs) |
| [**Level 04**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-04-advanced-integration.md) | **Advanced Input Modalities** | Bulk directory loading, `Stream`, `ReadOnlySpan<byte>`, and async cancellation | [`Level04AdvancedIntegration.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level04AdvancedIntegration.cs) |
| [**Level 05**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-05-processing-and-concurrency.md) | **High Concurrency & Cooperative Cancellation** | Stress testing under 200 concurrent tasks and early pipeline cancellation | [`Level05ProcessingAndConcurrency.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level05ProcessingAndConcurrency.cs) |
| [**Level 06**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-06-error-handling-and-classification.md) | **Error Handling Taxonomy & Attack Neutralization** | Classification of missing schemas, violations, syntax failures, and XXE attacks | [`Level06ErrorHandlingAndClassification.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level06ErrorHandlingAndClassification.cs) |
| [**Level 07**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-07-scalability-and-throughput.md) | **Micro-Benchmarks & Zero-Allocation Throughput** | Empirical allocation metrics and high-throughput memory profiling | [`Level07ScalabilityAndThroughput.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level07ScalabilityAndThroughput.cs) |
| [**Level 08**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-08-customization-and-extensibility.md) | **Extensibility via Decorator Pattern** | Intercepting `IXmlSchemaValidator` for telemetry, auditing, and fallback caching | [`Level08CustomizationAndExtensibility.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level08CustomizationAndExtensibility.cs) |
| [**Level 09**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-09-architectural-boundaries-and-consumers.md) | **Architectural Boundaries & Queue Consumers** | Clean architecture segregation, background workers, and boundary guards | [`Level09ArchitecturalBoundariesAndConsumers.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level09ArchitecturalBoundariesAndConsumers.cs) |
| [**Level 10**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase/level-10-enterprise-architecture.md) | **Enterprise Perimeter Defensive Pipeline** | Frontline DMZ gateway architecture neutralizing attacks before domain ingestion | [`Level10EnterpriseArchitecture.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level10EnterpriseArchitecture.cs) |

### 📖 Technical Reference & Architecture Guides

- [**Architecture Guide**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/architecture-guide.md) — System invariants, architectural boundaries, and component responsibilities.
- [**Architecture Flow & State Diagrams**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/architecture-flow.md) — Formal Mermaid pipeline diagrams, sequence diagrams, and lifecycle state machines.
- [**Architectural Decision Records (ADRs)**](https://github.com/ericksonlopezf/dotnet-xml/tree/main/docs/adr) — 17 formal records documenting decisions on Anti-XXE, Result types, caching, and Native AOT.
- [**Testing Roadmap & Quality Audit**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/testing-roadmap.md) — Canonical test taxonomy, 100% coverage audit, and Stryker mutation testing specification.
- [**API Inventory & Member Catalog**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/api-inventory.md) — Complete inventory of public interfaces, classes, extensions, and members.
- [**API Reference Guide**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/api-reference.md) — Microsoft Learn style reference with parameters, exceptions, and remarks.
- [**Performance Guide & Benchmarks**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/performance-guide.md) — Allocation analysis, concurrency scalability, and BenchmarkDotNet results.
- [**Cookbook & Production Recipes**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/cookbook.md) — 10 production-tested recipes for ASP.NET Core, streaming, and resilience.
- [**Migration Guide**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/migration-guide.md) — Step-by-step migration guide from legacy BCL `XmlReader` and `XmlSchemaSet`.
- [**Package Specification & Ecosystem**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/package-ecosystem.md) — Central Package Management, framework matrix, and packaging metadata.
- [**Troubleshooting Guide**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/troubleshooting.md) — Diagnosis and remediation for error codes, syntax errors, and schema violations.
- [**Showcase Execution Guide**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/showcase-guide.md) — Step-by-step instructions for executing and auditing the showcase project.
- [**Build & Quality Guide**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/build-and-quality.md) — MSBuild settings, compiler properties, Native AOT flags, and CI quality gates.

---

## 📥 Installation

### 1. Core Package (Required)

Install the package via the .NET CLI:

```bash
dotnet add package EricksonLopez.Xml.Validation
```

Or via the Visual Studio Package Manager Console:

```powershell
Install-Package EricksonLopez.Xml.Validation
```

Or reference it directly in your project file (`.csproj`):

```xml
<PackageReference Include="EricksonLopez.Xml.Validation" Version="1.0.0" />
```

### 2. Optional Framework & Integration Packages

All Microsoft Dependency Injection, Options, and Logging extensions are embedded directly in `EricksonLopez.Xml.Validation` without requiring auxiliary satellite packages. Railway-Oriented Programming support is provided transitively via `EricksonLopez.Result` (v2.0.0+).

### 3. Testing & Assertion Packages

When validating XML workflows in consumer test suites, assert on `Result<bool>` outcomes using `AwesomeAssertions` or standard xUnit/NUnit/MSTest assertions:

```bash
dotnet add package AwesomeAssertions
```

---

## 🚀 Quick Start

### 1. Standalone Setup & Schema Registration

```csharp
using System;
using System.IO;
using EricksonLopez.Xml.Validation;

// 1. Initialize the thread-safe schema cache
var cache = new XmlSchemaCache();

// 2. Register schema from string, stream, span, or file
string xsdContent = File.ReadAllText("orders.xsd");
cache.RegisterSchema("https://example.com/orders", xsdContent);

// 3. Initialize validator instance
var validator = new XmlSchemaValidator(cache);
```

### 2. Validating Documents via Result Pattern

```csharp
string xmlPayload = File.ReadAllText("order.xml");

// Validate against the registered targetNamespace
var result = validator.Validate(xmlPayload, "https://example.com/orders");

if (result.IsSuccess)
{
    Console.WriteLine("Document successfully validated against XSD schema!");
}
else
{
    // Typed error with code, category, and line/column coordinates
    Console.WriteLine($"Validation Failed [{result.Error.Code}]: {result.Error.Description}");
}
```

### 3. Reduced-Allocation Validation with ReadOnlySpan&lt;byte&gt;

For high-throughput pipelines, validate UTF-8 byte spans directly without materializing an intermediate managed `string` on the heap. The span is pinned via `fixed` and wrapped in an `UnmanagedMemoryStream`, so no payload buffer copy occurs. Note: BCL `XmlReader` internal parsing buffers (~2.16 KB) still allocate on Gen 0.

```csharp
ReadOnlySpan<byte> utf8XmlBytes = File.ReadAllBytes("order.xml");

// Validates directly over UTF-8 bytes — no managed string materialization,
// no payload array copy. BCL XmlReader internal buffers still allocate (~2.16 KB).
var result = validator.Validate(utf8XmlBytes, "https://example.com/orders");

if (result.IsFailure)
{
    Console.WriteLine($"Error: {result.Error.Description}");
}
```

### 4. Error Pattern Matching & HTTP RFC 9457 Mapping

```csharp
using Microsoft.AspNetCore.Http;
using EricksonLopez.Result;

var result = validator.Validate(xmlPayload, "https://example.com/orders");

if (result.IsFailure)
{
    return result.Error.Code switch
    {
        "XmlValidation.SchemaNotRegistered" => Results.NotFound(new { error = result.Error.Description }),
        "XmlValidation.XmlMalformed"        => Results.BadRequest(new { error = result.Error.Description }),
        "XmlValidation.SchemaViolation"     => Results.UnprocessableEntity(new { error = result.Error.Description }),
        _                                   => Results.StatusCode(StatusCodes.Status500InternalServerError)
    };
}
```

---

## 💡 Core Use Cases

### Use Case 1: Perimeter Defensive Gateway in ASP.NET Core Minimal APIs

Deploy `IXmlSchemaValidator` as an edge security boundary to sanitize and validate incoming XML payloads before handing them to typed business binders:

```csharp
app.MapPost("/api/invoices", async (
    HttpRequest request,
    IXmlSchemaValidator validator,
    CancellationToken ct) =>
{
    // Enable buffering so the request body stream can be rewound after validation
    request.EnableBuffering();

    // Validate stream directly without buffering as string
    var result = await validator.ValidateAsync(
        request.Body,
        "https://ericksonlopez.dev/invoice",
        ct);

    if (result.IsFailure)
    {
        return result.Error.Code switch
        {
            "XmlValidation.SchemaViolation" => Results.UnprocessableEntity(result.Error.Description),
            "XmlValidation.XmlMalformed"    => Results.BadRequest(result.Error.Description),
            _                               => Results.Problem(result.Error.Description, statusCode: 500)
        };
    }

    // Reset stream position for the typed deserializer (stream left open by validator; requires EnableBuffering)
    request.Body.Position = 0;
    var invoice = await DeserializeInvoiceAsync(request.Body, ct);
    return Results.Ok(invoice);
});
```

### Use Case 2: Asynchronous HTTP Body Stream Validation with Cooperative Cancellation

Validate large XML streams arriving over the network without blocking worker threads, honoring cancellation tokens during I/O and parse loops:

```csharp
public async ValueTask<Result<bool>> ProcessStreamPayloadAsync(
    Stream networkStream,
    string targetNamespace,
    CancellationToken cancellationToken)
{
    // Validates asynchronously; leaves the stream open for subsequent downstream consumers
    return await _validator.ValidateAsync(
        networkStream,
        targetNamespace,
        cancellationToken);
}
```

### Use Case 3: High-Throughput In-Memory Queue Buffer Processing

Process high-volume messages from Kafka or RabbitMQ directly from byte buffers with minimal memory overhead:

```csharp
public void OnMessageReceived(ReadOnlySpan<byte> messageBuffer)
{
    // Zero string allocation hot path
    var result = _validator.Validate(messageBuffer, "https://ericksonlopez.dev/orders");

    if (result.IsFailure)
    {
        _deadLetterPublisher.Publish(messageBuffer, result.Error);
        return;
    }

    _queueDispatcher.Dispatch(messageBuffer);
}
```

### Use Case 4: Bulk Schema Precompilation on Application Startup

Scan schema repositories and precompile hundreds of XSD documents into the cache before receiving traffic:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddXmlValidation();

var app = builder.Build();

// Preload all enterprise schemas recursively at host boot
var cache = app.Services.GetRequiredService<IXmlSchemaCache>();
string schemasPath = Path.Combine(AppContext.BaseDirectory, "Schemas");
int precompiledCount = cache.RegisterSchemasFromDirectory(schemasPath, "*.xsd");

app.Logger.LogInformation("Successfully precompiled {Count} XSD schemas.", precompiledCount);
app.Run();
```

### Use Case 5: Strict Regulatory Compliance with Warning Escalation

Enforce strict financial or healthcare schemas (e.g., UBL 2.1 or ISO 20022) where any schema warning must be treated as a rejection:

```csharp
builder.Services.AddXmlValidation(options =>
{
    // In compliance mode, schema warnings are captured and escalated to validation failure
    options.IncludeWarnings = true;
    options.TreatWarningsAsErrors = true;
});
```

### Use Case 6: Custom Telemetry and Metrics Enrichment via Decorator Pattern

Wrap `IXmlSchemaValidator` with a decorator to capture OpenTelemetry metrics, execution durations, and custom security audits:

```csharp
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;

public sealed class TelemetryXmlValidatorDecorator : IXmlSchemaValidator
{
    private readonly IXmlSchemaValidator _inner;

    // Meter must be instantiated; it is not a static class.
    private static readonly Meter _meter = new("EricksonLopez.Xml.Validation", "1.0");
    private static readonly Counter<long> ValidationFailures =
        _meter.CreateCounter<long>("xml_validation_failures_total");

    public TelemetryXmlValidatorDecorator(IXmlSchemaValidator inner) => _inner = inner;

    public Result<bool> Validate(string xmlContent, string targetNamespace)
    {
        var result = _inner.Validate(xmlContent, targetNamespace);
        if (result.IsFailure)
        {
            ValidationFailures.Add(1, new KeyValuePair<string, object?>("error_code", result.Error.Code));
        }
        return result;
    }

    public Result<bool> Validate(Stream xmlStream, string targetNamespace) =>
        _inner.Validate(xmlStream, targetNamespace);

    public Result<bool> Validate(ReadOnlySpan<byte> utf8XmlBytes, string targetNamespace) =>
        _inner.Validate(utf8XmlBytes, targetNamespace);

    public Task<Result<bool>> ValidateAsync(
        Stream xmlStream,
        string targetNamespace,
        CancellationToken cancellationToken = default) =>
        _inner.ValidateAsync(xmlStream, targetNamespace, cancellationToken);
}
```

---

## 🔌 Configuration & Integrations

### Microsoft Dependency Injection Integration

`EricksonLopez.Xml.Validation` provides first-class support for `Microsoft.Extensions.DependencyInjection`:

```csharp
using EricksonLopez.Xml.Validation;

// Register default singletons
builder.Services.AddXmlValidation();

// Or configure options explicitly covering the full options surface
builder.Services.AddXmlValidation(options =>
{
    options.IncludeWarnings = true;              // Captures warnings in error descriptions
    options.TreatWarningsAsErrors = false;       // Warnings will not fail validation unless true
    options.MaxCharactersInDocument = 5_000_000; // Defense against XML DoS
    options.MaxErrors = 50;                      // Cap error collection against memory exhaustion
    options.ProcessInlineSchema = false;         // Defense against inline schema poisoning
});
```

The extension registers both `IXmlSchemaCache` (`XmlSchemaCache`) and `IXmlSchemaValidator` (`XmlSchemaValidator`) as **Singletons** in the container.

### Zero-Allocation Diagnostic Logging

When `ILogger<XmlSchemaValidator>` is registered in the service provider, `XmlSchemaValidator` emits structured, source-generated diagnostic events via compile-time `[LoggerMessage]` partial methods:

| Event ID | Level | Event Name | Description |
|:---:|:---:|:---|:---|
| `1001` | `Debug` | `SchemaNotRegistered` | Emitted when requested `targetNamespace` is missing from `IXmlSchemaCache`. |
| `1002` | `Debug` | `ValidationFailed` | Emitted when document violates schema rules (contains line/column data). |
| `1003` | `Debug` | `ValidationSucceeded` | Emitted when document passes all schema validation constraints. |
| `1004` | `Debug` | `ValidationCancelled` | Emitted when validation loop is aborted via `CancellationToken`. |
| `1005` | `Debug` | `MalformedXml` | Emitted on XML syntax errors or prohibited DTD/XXE injection attempts. |

### Native AOT & Trimming Compatibility

`EricksonLopez.Xml.Validation` is verified trim-safe and Native AOT compatible:

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
</PropertyGroup>
```

- **Zero Reflection on Hot Paths:** Avoids dynamic code generation and unconstrained reflection.
- **Source-Generated Logging:** Utilizes compile-time logging delegates.
- **AOT Verification:** Continuously audited by the `tests/EricksonLopez.Xml.Validation.AotTest` smoke-test suite.

---

## 🧪 Testing & Quality

The `EricksonLopez.Xml.Validation` test harness is engineered to enforce absolute correctness, boundary safety, and regression resistance.

### Authoritative Quality Gates Matrix

| Metric | Target | Initial State | Verified Final State | Status |
|---|:---:|:---:|:---:|:---:|
| **Line Coverage** | **≥ 100%** | 100% | **100.00% (260/260 lines)** | **COMPLIANT** |
| **Branch Coverage** | **≥ 100%** | 100% | **100.00% (46/46 branches)** | **COMPLIANT** |
| **Method Coverage** | **≥ 100%** | 100% | **100.00% (24/24 methods)** | **COMPLIANT** |
| **Mutation Score** | **100%** | 96.58% | **100.00% (117 killed / 0 survived)** | **COMPLIANT** |

### Testing Pyramid & Taxonomy

The solution is divided into segregated testing projects targeting distinct quality concerns:

- **Unit & Integration Tests (`tests/EricksonLopez.Xml.Validation.Tests`)**: 193 tests executed across all 3 target frameworks (`net8.0`, `net9.0`, `net10.0`), totaling **579 test runs passing** with 0 failures. Covers synchronous and asynchronous validation, multi-modal inputs, Anti-XXE enforcement, warning configurations, DoS limits, and DI lifecycles.
- **Architecture Tests (`tests/EricksonLopez.Xml.Validation.ArchitectureTests`)**: 6 tests executed across all 3 target frameworks (**18 test runs passing**), validating design invariants using `NetArchTest.Rules` (sealed public classes, absence of obsolete types, namespace boundaries, and interface contracts).
- **Native AOT Smoke Test (`tests/EricksonLopez.Xml.Validation.AotTest`)**: Compiles and executes against standalone native binaries using `PublishAot=true`.

### Stryker.NET Mutation Testing

Mutation testing is executed against the core validation engine to ensure test assertions are mathematically sensitive to state mutations:

```json
{
  "stryker-config": {
    "thresholds": {
      "high": 100,
      "low": 98,
      "break": 95
    }
  }
}
```

*Results:* **100.00% Mutation Score** (117 mutants killed, 0 mutants survived).

---

## ⚡ Performance Benchmarks

> **Environment:** BenchmarkDotNet v0.15.8, Windows 11, AMD Ryzen 7 9800X3D 4.70GHz, .NET 10.0.11, X64 RyuJIT x86-64-v4

### Primary Operations Benchmark

Measurements collected on validating an invoice payload against an enterprise XSD schema:

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|---|---:|---:|---:|:---:|---:|---:|:---:|
| `ValidateFromString` | 3,029.05 ns | 447.52 ns | 24.53 ns | 1.000 | 1.5030 | 75.66 KB | 1.00 |
| `ValidateFromStream` | 3,841.48 ns | 572.26 ns | 31.37 ns | 1.268 | 2.1591 | 108.51 KB | 1.43 |
| `ValidateFromSpan` | 3,346.45 ns | 1,768.48 ns | 96.94 ns | 1.105 | 2.1591 | 108.59 KB | 1.44 |
| `CacheLookupContainsSchema` | 6.31 ns | 0.44 ns | 0.02 ns | 0.002 | - | **0 B** | **0.00** |

### Precompiled Cache vs Ad-Hoc BCL Compilation

Execution time comparison over 1,000 iterations:

| Approach | Total Time | Average Time / Op | GC Allocations | Speedup |
|---|---:|---:|:---|:---:|
| **Ad-Hoc BCL Compilation** (`new XmlSchemaSet().Compile()`) | ~47 ms | 47 µs | High (Schema graph + Gen 0/1 pressure) | Baseline |
| **Precompiled `IXmlSchemaCache`** | ~9 ms | 9 µs | Minimal (Validation state machine only) | **5x to 100x Faster** |

> [!NOTE]
> **These figures are representative estimates** (5x–10x for simple schemas, up to 100x for complex enterprise schemas like UBL 2.1 or ISO 20022). They are **not produced by `XmlValidationBenchmarks.cs`** — that harness benchmarks input modalities against a pre-registered cache. To produce a reproducible ad-hoc vs. precompiled comparison, extend the harness with a `[Benchmark]` that calls `new XmlSchemaSet().Add(...).Compile()` before each validation. See [performance-guide.md](https://github.com/ericksonlopezf/dotnet-xml/blob/main/docs/performance-guide.md) for details.

---

## 🌐 Compatibility & Technical Matrix

### Target Frameworks & Runtime Matrix

| Package | .NET 8.0 LTS | .NET 9.0 STS | .NET 10.0 | Native AOT | Trimmable | Notes |
|---|:---:|:---:|:---:|:---:|:---:|---|
| `EricksonLopez.Xml.Validation` | ✅ Compatible | ✅ Compatible | ✅ Compatible | ✅ Verified | ✅ Verified | Zero reflection, `<IsAotCompatible>true</IsAotCompatible>` |

### Target Framework & Lifecycle Policy

> 🛡️ **Target Framework & Lifecycle Policy**: First-class multi-targeting across `.NET 10` (Modern LTS), `.NET 9` (STS), and `.NET 8` (Enterprise LTS) is actively maintained. Full backward compatibility is guaranteed until Microsoft officially reaches End-of-Life (EOL) for .NET 8 and .NET 9 in November 2026, at which milestone the ecosystem will transition to .NET 10 and .NET 11.

### Error Taxonomy & RFC 9457 Problem Details Mapping

All failures produce structured `Result<bool>` error descriptors with standardized error codes:

| Error Code | Category | HTTP Status (RFC 9457) | Architectural Root Cause |
|---|---|:---:|---|
| `XmlValidation.SchemaNotRegistered` | `ErrorType.NotFound` | `404 Not Found` | The requested `targetNamespace` was not found in `IXmlSchemaCache`. |
| `XmlValidation.XmlMalformed` | `ErrorType.Validation` | `400 Bad Request` | Document is syntactically malformed, unclosed, or contains prohibited DTD/XXE structures. |
| `XmlValidation.SchemaViolation` | `ErrorType.Validation` | `422 Unprocessable Entity` | Document syntax is valid XML but violates elements, attributes, or type constraints of the XSD schema. |

---

## 🏛️ Architecture & Design Principles

### Validation Decision Pipeline

The following flowchart details the decision pipeline and error branching executed on every validation invocation:

```mermaid
flowchart TD
    Start(["Start: Validate / ValidateAsync Invocation"]) --> CheckCache{"Does targetNamespace exist\nin IXmlSchemaCache?"}

    CheckCache -- "No" --> RetNotFound["Return Error.NotFound\n'XmlValidation.SchemaNotRegistered'\n(EventId 1001)"]
    RetNotFound --> Done(["End"])

    CheckCache -- "Yes" --> PrepSettings["Configure XmlReaderSettings:\n• DtdProcessing = Prohibit (Anti-XXE)\n• XmlResolver = null\n• ValidationType = Schema\n• Schemas = XmlSchemaSet\n• MaxCharactersInDocument\n• Attach ValidationEventHandler"]

    PrepSettings --> CreateReader["Create XmlReader over Input\n(String / Stream / ReadOnlySpan<byte>)"]
    CreateReader --> ReadLoop{"XmlReader.Read()\n/ ReadAsync()"}

    ReadLoop -- "XmlException\n(Syntax error, DTD, or DoS limit)" --> RetMalformed["Return Error.Validation\n'XmlValidation.XmlMalformed'\n(EventId 1005)"]
    RetMalformed --> Done

    ReadLoop -- "First Element Node" --> CheckRoot{"IsRootElementDeclared\nin schemaCache?"}
    CheckRoot -- "No" --> AddRootError["Append Error:\nRoot element not declared"]
    CheckRoot -- "Yes" --> ContinueRead["Continue Streaming"]
    AddRootError --> ReadLoop
    ContinueRead --> ReadLoop

    ReadLoop -- "ValidationEventHandler Error" --> AddError["Append Error with Line/Position\n(up to MaxErrors)"]
    AddError --> ReadLoop

    ReadLoop -- "ValidationEventHandler Warning" --> CheckWarnConfig{"options.IncludeWarnings?"}
    CheckWarnConfig -- "Yes" --> AddWarn["Append Warning with '[Warning]' prefix"]
    CheckWarnConfig -- "No" --> IgnoreWarn["Silently ignore warning"]
    AddWarn --> ReadLoop
    IgnoreWarn --> ReadLoop

    ReadLoop -- "End of Document (EOF)" --> EvaluateErrors{"Errors > 0 OR\n(TreatWarningsAsErrors && Warnings > 0)?"}

    EvaluateErrors -- "Yes" --> RetViolation["Return Error.Validation\n'XmlValidation.SchemaViolation'\n(EventId 1002)"]
    EvaluateErrors -- "No" --> RetSuccess["Return Result<bool>.Success(true)\n(EventId 1003)"]

    RetViolation --> Done
    RetSuccess --> Done
```

### Validation Lifecycle State Machine

```mermaid
stateDiagram-v2
    [*] --> Received: Validate(input, ns)
    Received --> CheckingCache: Query IXmlSchemaCache

    CheckingCache --> SchemaNotRegistered: Schema not found in cache
    SchemaNotRegistered --> [*]: Returns Error.NotFound

    CheckingCache --> InitializingReader: Precompiled SchemaSet resolved
    InitializingReader --> ParsingNodes: Anti-XXE XmlReaderSettings applied

    state ParsingNodes {
        [*] --> ReadingNode
        ReadingNode --> AccumulatingErrors: XSD validation error event
        AccumulatingErrors --> ReadingNode: Continue stream
        ReadingNode --> EvaluatingWarning: XSD warning event
        EvaluatingWarning --> AccumulatingWarnings: IncludeWarnings = true
        EvaluatingWarning --> ReadingNode: IncludeWarnings = false
        AccumulatingWarnings --> ReadingNode
        ReadingNode --> Malformed: DTD or syntax violation
        ReadingNode --> Cancelled: CancellationToken triggered
        ReadingNode --> Completed: EOF reached
    }

    Malformed --> [*]: Returns Error.Validation (XmlMalformed)
    Cancelled --> [*]: Throws OperationCanceledException
    Completed --> EvaluatingResult

    EvaluatingResult --> ValidationFailed: Errors > 0 or (TreatWarningsAsErrors && Warnings > 0)
    EvaluatingResult --> ValidationSucceeded: No blocking errors or warnings

    ValidationFailed --> [*]: Returns Error.Validation (SchemaViolation)
    ValidationSucceeded --> [*]: Returns Result.Success(true)
```

### Defensive Perimeter Ingestion Pipeline

```mermaid
flowchart LR
    UntrustedPayload["Untrusted XML Payload\n(WebHook / HTTP Body / Queue Buffer)"] --> Firewall["Perimeter Defensive Gateway\n(IXmlSchemaValidator)"]

    Firewall -- "XXE Attack / DTD Prohibited" --> RejectXXE["400 Bad Request\nImmediately Neutralized\nZero Entity Expansion"]
    Firewall -- "XSD Schema Violation" --> RejectSchema["422 Unprocessable Entity\nRejected with Line/Position"]
    Firewall -- "Compliant & Safe" --> SafeParser["Secure Typed Deserializer\n(Domain Command / Entity)"]

    SafeParser --> DomainCore["Domain Core / CQRS Handlers\n(Guaranteed Valid and XXE-Free XML)"]
```

---

## 🛡️ Best Practices & Anti-Patterns

### Recommended vs Avoid

| Scenario | ❌ Avoid | ✅ Recommended |
|---|---|---|
| **Security Defaults** | Configuring `DtdProcessing.Parse` or assigning non-null `XmlResolver` | Relying on `XmlSchemaValidator` defaults (`DtdProcessing.Prohibit`, `XmlResolver = null`) |
| **Control Flow** | Throwing and catching `XmlException` or `XmlSchemaValidationException` | Evaluating functional `Result<bool>` returns without exceptions |
| **Schema Compilation** | Calling `new XmlSchemaSet().Compile()` on incoming HTTP requests | Preloading schemas into `IXmlSchemaCache` as a singleton at startup |
| **Memory Allocation** | Converting raw `byte[]` buffers into `string` prior to validation | Validating directly over `ReadOnlySpan<byte>` or streaming input |
| **Stream Management** | Disposing or buffering the entire `Stream` before validation completes | Passing open `Stream` instances directly; validator preserves stream ownership |
| **Diagnostic Logging** | String interpolation in log statements (`logger.LogInformation($"...")`) | Utilizing source-generated `[LoggerMessage]` structured events |
| **Warning Handling** | Silently ignoring XSD schema warnings in regulatory scenarios | Configuring `options.IncludeWarnings = true` and `options.TreatWarningsAsErrors = true` |

---

## ⚠️ Troubleshooting & Common Pitfalls

> [!CAUTION]
> The default security configuration intentionally rejects all DTD declarations and external references. Attempting to pass documents containing `<!DOCTYPE ...>` will result in immediate validation failure.

### 1. `XmlValidation.SchemaNotRegistered`
- **Symptom:** Validation returns `Error.NotFound` with code `XmlValidation.SchemaNotRegistered`.
- **Root Cause:** The `targetNamespace` passed to `validator.Validate(...)` does not match the namespace registered in `IXmlSchemaCache`.
- **Resolution:** Verify exact URI string casing. Preload the schema during application startup using `cache.RegisterSchema("https://exact-uri", xsdContent)`.

### 2. `XmlValidation.XmlMalformed` with DTD Detected
- **Symptom:** Payload fails validation with description indicating DTD processing is prohibited.
- **Root Cause:** The input document contains a `<!DOCTYPE ...>` declaration, which is prohibited to eliminate XXE vulnerabilities (CWE-611).
- **Resolution:** Strip all DTD declarations from the XML before sending it to the validator, or verify that the sending client is compliant with modern pure-XSD standards.

### 3. Stream Closed or Consumed Before Validation
- **Symptom:** `Validate(stream, ...)` fails with unexpected end of file or malformed XML.
- **Root Cause:** Upstream middleware or deserializers read the `Stream` to position `Length` without rewinding.
- **Resolution:** Reset the stream position (`stream.Position = 0;`) before calling `validator.Validate(...)` or `validator.ValidateAsync(...)`.

### 4. Schema Warnings Not Causing Validation Failure
- **Symptom:** Documents with schema warnings return `Result.IsSuccess == true`.
- **Root Cause:** By default, `IncludeWarnings` is `false` and `TreatWarningsAsErrors` is `false`.
- **Resolution:** Set `options.IncludeWarnings = true;` and `options.TreatWarningsAsErrors = true;` when registering via `services.AddXmlValidation(options => ...)`.

---

## 🌐 Part of the EricksonLopez Ecosystem

`EricksonLopez.Xml.Validation` is an integral component of the EricksonLopez enterprise foundation library ecosystem:

- ⚡ [**EricksonLopez.Result**](https://github.com/ericksonlopezf/dotnet-result) — High-performance, struct-based Result pattern and Railway-Oriented Programming ecosystem.
- 🧱 [**EricksonLopez.SharedKernel**](https://github.com/ericksonlopezf/dotnet-shared-kernel) — Foundational enterprise domain primitives, value objects, specifications, and domain events.
- 🔒 [**EricksonLopez.Security**](https://github.com/ericksonlopezf/dotnet-security) — Enterprise zero-trust security framework, cryptographic primitives, PKI, and authentication.
- 🔁 [**EricksonLopez.Idempotency**](https://github.com/ericksonlopezf/dotnet-idempotency) — Enterprise distributed idempotency engine with database-backed coordination.
- 🏢 [**EricksonLopez.MultiTenancy**](https://github.com/ericksonlopezf/dotnet-multitenancy) — Multi-tenant isolation architecture and PostgreSQL Row-Level Security integration.
- ⏱️ [**EricksonLopez.Concurrency**](https://github.com/ericksonlopezf/dotnet-concurrency) — Zero-allocation optimistic concurrency control and version-checked state transitions.
- 💳 [**EricksonLopez.Transaction**](https://github.com/ericksonlopezf/dotnet-transaction) — Resilient database transaction orchestration and execution strategy manager.
- 📡 [**EricksonLopez.Mediator**](https://github.com/ericksonlopezf/dotnet-mediator) — Zero-allocation, struct-based mediator and in-process messaging pipeline.
- 🔍 [**EricksonLopez.Specification**](https://github.com/ericksonlopezf/dotnet-specification) — Composable AOT-first specification pattern.

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Please review our community guidelines before submitting pull requests:

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0), and [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 (v17.12+) or JetBrains Rider (2024.3+)

### Local Development Commands

```bash
# Clone the repository
git clone https://github.com/ericksonlopezf/dotnet-xml.git
cd dotnet-xml

# Restore dependencies
dotnet restore

# Build with deterministic warnings-as-errors enforcement
dotnet build --configuration Release

# Execute comprehensive unit, integration, and architecture tests
dotnet test --configuration Release

# Execute Native AOT verification smoke test
dotnet run --project tests/EricksonLopez.Xml.Validation.AotTest/EricksonLopez.Xml.Validation.AotTest.csproj --configuration Release

# Run mutation testing with Stryker.NET
dotnet stryker --config-file stryker-config.json
```

### Community Resources

- [**Contributing Guidelines**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/CONTRIBUTING.md) — Coding conventions, Git workflows, and commit standards.
- [**Code of Conduct**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/CODE_OF_CONDUCT.md) — Contributor Covenant v2.1 standards.
- [**Security Policy**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/SECURITY.md) — Responsible vulnerability disclosure guidelines.
- [**Support Policy**](https://github.com/ericksonlopezf/dotnet-xml/blob/main/SUPPORT.md) — Getting help, feature requests, and bug reporting.

---

## 📄 License

Distributed under the [MIT License](https://github.com/ericksonlopezf/dotnet-xml/blob/main/LICENSE). Copyright © 2026 Erickson Lopez.
