# ADR-009: Zero-Allocation Diagnostic Logging via LoggerMessage Attribute

- **Status:** Accepted
- **Date:** 2026-09-02
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

Traditional `ILogger.LogInformation` or `ILogger.LogWarning` calls box value-type parameters (such as line numbers, positions, and elapsed milliseconds) and allocate string formatting buffers even when the target log level is disabled.

## Decision

All diagnostic logging throughout `XmlSchemaValidator` uses the Roslyn compile-time source generator attribute `[LoggerMessage]`:

```csharp
[LoggerMessage(EventId = 1001, Level = LogLevel.Debug,
    Message = "Schema not registered for namespace '{Namespace}'")]
private static partial void LogSchemaNotRegistered(ILogger logger, string @namespace);

[LoggerMessage(EventId = 1005, Level = LogLevel.Debug,
    Message = "Malformed XML at Line {LineNumber}, Position {LinePosition} for namespace '{Namespace}': {XmlMessage}")]
private static partial void LogMalformedXml(
    ILogger logger, string @namespace, int lineNumber, int linePosition, string xmlMessage);
```

## Consequences

### Positive
- Zero boxing of numeric line and column coordinates.
- Zero string formatting allocations when the corresponding log level is disabled.
- Strongly-typed structured event IDs for telemetry filtering.

### Negative
- Requires declaring classes as `partial`.
