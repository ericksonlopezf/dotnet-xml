# Level 05: Concurrent Processing & Cooperative Cancellation

## Overview

High-scale ingestion backplanes process thousands of concurrent XML payloads across thread pool workers. Thread safety, lock contention, and timely cancellation under client disconnects are mission-critical requirements.

---

## 1. Thread-Safe Singleton Architecture

`XmlSchemaCache` and `XmlSchemaValidator` are strictly thread-safe and designed for registration as **Singletons** in the dependency injection container.

```mermaid
flowchart TD
    subgraph Multi-Threaded Ingestion
        T1["Task 1 (Worker Thread)"]
        T2["Task 2 (Worker Thread)"]
        TN["Task N (Worker Thread)"]
    end

    SingletonValidator["IXmlSchemaValidator (Singleton)"]
    ConcurrentCache["IXmlSchemaCache (ConcurrentDictionary Lock-Free)"]

    T1 -->|Validate| SingletonValidator
    T2 -->|Validate| SingletonValidator
    TN -->|Validate| SingletonValidator

    SingletonValidator -->|Query SchemaSet O(1)| ConcurrentCache
```

- **Lock-Free Read-Through**: Precompiled `XmlSchemaSet` instances stored in `ConcurrentDictionary<string, SchemaCacheEntry>` are immutable after compilation and shared concurrently without locking overhead. (`SchemaCacheEntry` is a private sealed record wrapping the compiled `XmlSchemaSet`.)
- **Thread Isolation**: Each validation invocation constructs its own local `XmlReader` over the input stream/span, ensuring zero mutable shared state between parallel requests.

---

## 2. Cooperative Asynchronous Cancellation

Long-running or stalled network streams must be cancellable to prevent connection pool exhaustion and thread starvation:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

try
{
    Result<bool> result = await validator.ValidateAsync(
        networkStream,
        "https://example.com/schema",
        cts.Token);
}
catch (OperationCanceledException)
{
    // Clean cancellation: Stream state aborted gracefully without resource leaks
}
```

- When the `CancellationToken` signals cancellation during async I/O or XML tokenization, `ValidateAsync` terminates immediately with `OperationCanceledException`.

---

## Code Reference
- [`Level05ProcessingAndConcurrency.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level05ProcessingAndConcurrency.cs)
