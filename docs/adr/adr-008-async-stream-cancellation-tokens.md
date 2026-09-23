# ADR-008: Asynchronous Streaming Validation with Cooperative Cancellation

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

Validating multi-megabyte XML files synchronously blocks calling threads, creating thread-pool starvation under high concurrent load in web applications. Furthermore, long-running validations must honor client disconnects and timeout bounds to prevent resource exhaustion attacks.

## Decision

We expose asynchronous stream validation that accepts a mandatory `CancellationToken`:

```csharp
public Task<Result<bool>> ValidateAsync(
    Stream xmlStream,
    string targetNamespace,
    CancellationToken cancellationToken = default);
```

The implementation configures `XmlReaderSettings.Async = true` and performs cooperative cancellation checks during reader advancement.

## Consequences

### Positive
- Non-blocking I/O execution suitable for ASP.NET Core request pipelines.
- Immediate resource reclaim when clients abort HTTP requests or timeouts trigger.
- Protects services against denial-of-service through oversized XML payloads.

### Negative
- Asynchronous XML reader state machines incur minor async state machine allocation overhead compared to raw synchronous readers.
