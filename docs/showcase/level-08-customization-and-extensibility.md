# Level 08: Customization & Extensibility via Decorators

## Overview

Modern cloud architectures mandate rich telemetry, execution timing, security auditing, and distributed metrics. `EricksonLopez.Xml.Validation` supports non-invasive extensibility through standard Gang-of-Four **Decorator** patterns over `IXmlSchemaValidator`.

---

## 1. Decorating `IXmlSchemaValidator`

Because `IXmlSchemaValidator` is defined as a pure application contract, cross-cutting concerns (auditing, OpenTelemetry counters, duration histograms) can be injected cleanly without altering core validation logic or breaking Native AOT compatibility.

```csharp
public sealed class AuditingXmlSchemaValidator : IXmlSchemaValidator
{
    private readonly IXmlSchemaValidator _inner;

    public AuditingXmlSchemaValidator(IXmlSchemaValidator inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public Result<bool> Validate(string xml, string targetNamespace)
    {
        var sw = Stopwatch.StartNew();
        var result = _inner.Validate(xml, targetNamespace);
        sw.Stop();

        // Emit audit or telemetry metrics
        Activity.Current?.SetTag("xml.validation.duration_us", sw.Elapsed.TotalMicroseconds);
        return result;
    }

    public Result<bool> Validate(Stream xmlStream, string targetNamespace) =>
        _inner.Validate(xmlStream, targetNamespace);

    public Result<bool> Validate(ReadOnlySpan<byte> utf8Xml, string targetNamespace) =>
        _inner.Validate(utf8Xml, targetNamespace);

    public Task<Result<bool>> ValidateAsync(Stream xmlStream, string targetNamespace, CancellationToken ct = default) =>
        _inner.ValidateAsync(xmlStream, targetNamespace, ct);
}
```

---

## 2. Dependency Injection Registration

Decorators are wired via factory delegates in `IServiceCollection`:

```csharp
services.AddXmlValidation();

// Wrap singleton validator with auditing decorator
services.AddSingleton<IXmlSchemaValidator>(sp =>
{
    var cache = sp.GetRequiredService<IXmlSchemaCache>();
    var coreValidator = new XmlSchemaValidator(cache);
    return new AuditingXmlSchemaValidator(coreValidator);
});
```

---

## Code Reference
- [`Level08CustomizationAndExtensibility.cs`](https://github.com/ericksonlopezf/dotnet-xml/blob/main/samples/EricksonLopez.Xml.Showcase/Levels/Level08CustomizationAndExtensibility.cs)
