# ADR-007: Reduced-Allocation ReadOnlySpan<byte> Validation Modality

- **Status:** Accepted (Amended)
- **Date:** 2026-09-02 (Amended: 2026-09-13)
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

High-throughput enterprise systems (such as electronic invoicing gateways, financial SWIFT/SEPA pipelines, and microservice message brokers) frequently receive XML payloads over the network as raw UTF-8 byte buffers.

In standard .NET implementations, parsing these payloads requires either:
1. Converting raw byte buffers into intermediate `string` instances (`Encoding.UTF8.GetString(bytes)`), which allocates managed strings on the Gen 0 heap and triggers GC collections proportional to throughput.
2. Allocating intermediate heap arrays (`bytes.ToArray()`) to back standard `MemoryStream` instances, which duplicates buffer memory and creates severe heap fragmentation.

## Decision

We expose a dedicated, high-performance validation overload accepting `ReadOnlySpan<byte>`:

```csharp
public unsafe Result<bool> Validate(ReadOnlySpan<byte> utf8Xml, string targetNamespace);
```

Internally, this implementation uses **fixed pointer pinning** and `UnmanagedMemoryStream`:

```csharp
fixed (byte* ptr = utf8Xml)
{
    using var memoryStream = new UnmanagedMemoryStream(ptr, utf8Xml.Length, utf8Xml.Length, FileAccess.Read);
    return ValidateStreamInternal(memoryStream, targetNamespace);
}
```

This ensures:
- **Zero GC Heap Allocations for Payload Memory**: The bytes are read directly from stackalloc, unmanaged, or pooled memory without copying to a managed byte array.
- **Direct Streaming Parser Execution**: `XmlReader.Create(Stream, XmlReaderSettings)` streams directly across the pinned memory block.
- **Unmanaged Lifetime Boundary**: Memory is pinned only for the duration of synchronous parsing within the scope of the `fixed` block.

## Consequences

### Positive
- **Zero Buffer Duplication**: 0 B Gen 0 allocations for payload data during span validation.
- **Low Latency Under Concurrency**: Drastically suppresses Gen 0/Gen 1 GC pause frequency in high-throughput API endpoints.
- **Native AOT Compliance**: `UnmanagedMemoryStream` and `XmlReader` streaming operate cleanly under Native AOT trimming without reflection.
- **Stackalloc & ArrayPool Compatibility**: Callers can pass memory slices from `ArrayPool<byte>.Shared` or stack-allocated buffers.

### Negative & Trade-Offs
- Requires `AllowUnsafeBlocks=true` in the project configuration to support the pinned pointer constructor of `UnmanagedMemoryStream`.
- Synchronous-only execution: `ReadOnlySpan<byte>` cannot cross asynchronous yield points, so async streaming continues to utilize standard `Stream`.
- The underlying BCL `XmlReader` allocates internal parsing and decoding state buffers on the Gen 0 heap (~2.16 KB per operation). While intermediate managed string allocations and payload buffer duplications are strictly 0 B, total operation memory is constrained by BCL parser internals.
