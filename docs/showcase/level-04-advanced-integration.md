# Level 04: Advanced Integration & Input Modalities

## Overview

Enterprise systems ingest XML across diverse protocols, runtimes, and memory structures: HTTP request bodies, message queues, raw socket buffers, and local filesystems. `EricksonLopez.Xml.Validation` supports 4 distinct input modalities designed for maximum performance, minimal heap allocations, and deterministic resource ownership.

---

## 1. Bulk Schema Precompilation from Filesystem

In microservices and modular monoliths, schemas are frequently organized in filesystem hierarchies. `IXmlSchemaCache` provides bulk directory discovery and recursive compilation:

```csharp
var cache = new XmlSchemaCache();
var schemasDir = Path.Combine(AppContext.BaseDirectory, "Schemas");

// Discovers and compiles all *.xsd files recursively, indexing by targetNamespace
int compiledCount = cache.RegisterSchemasFromDirectory(schemasDir, "*.xsd");
```

- **Idempotent Precompilation**: Each schema's `targetNamespace` is extracted and compiled once into an immutable `XmlSchemaSet`.
- **Startup Guarantee**: Guarantees that all dependent schemas are precompiled and cached before the service starts accepting traffic.

---

## 2. Multi-Modal Input Ingestion

### Modality 1: In-Memory Managed String
Standard synchronous validation for in-memory string payloads:

```csharp
Result<bool> result = validator.Validate(xmlString, "https://example.com/schema");
```

### Modality 2: Synchronous Stream
Validates directly from any readable `Stream` (File, Memory, Network) without converting to `string`:

```csharp
using var stream = File.OpenRead("document.xml");
Result<bool> result = validator.Validate(stream, "https://example.com/schema");
```
> [!NOTE]
> The validator leaves the stream open and preserves its position at completion, allowing downstream deserializers to consume the payload.

### Modality 3: High-Performance `ReadOnlySpan<byte>` (Zero Allocation)
Validates UTF-8 byte buffers directly from socket memory, pipeline readers, or Kafka buffers without allocating intermediate managed strings:

```csharp
ReadOnlySpan<byte> utf8Payload = GetPayloadBuffer();
Result<bool> result = validator.Validate(utf8Payload, "https://example.com/schema");
```

### Modality 4: Asynchronous Stream with Cooperative Cancellation
Non-blocking validation with `CancellationToken` support for async I/O pipelines:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
Result<bool> result = await validator.ValidateAsync(stream, "https://example.com/schema", cts.Token);
```

---

## Code Reference
- [`Level04AdvancedIntegration.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level04AdvancedIntegration.cs)
