# ADR-010: Singleton Service Lifetime for Schema Cache and Validator

- **Status:** Accepted (Amended)
- **Date:** 2026-09-02 (Amended: 2026-09-14)
- **Deciders:** EricksonLopez.Xml.Validation Architecture Team

---

## Context

Compiling XML Schema Definition (XSD) files is computationally expensive, involving lexical analysis, AST construction, and symbol table compilation. Instantiating `XmlSchemaCache` or `XmlSchemaValidator` on a per-request (transient/scoped) basis severely degrades throughput.

## Decision

The standard Dependency Injection registration method `services.AddXmlValidation()` registers `IXmlSchemaCache` and `IXmlSchemaValidator` as **Singletons** using `TryAddSingleton`, making the registration **idempotent**:

```csharp
public static IServiceCollection AddXmlValidation(
    this IServiceCollection services,
    Action<XmlValidationOptions>? configure)
{
    ArgumentNullException.ThrowIfNull(services);

    var options = new XmlValidationOptions();
    configure?.Invoke(options);                       // Apply caller configuration

    // TryAddSingleton is used intentionally: if the consumer has already
    // registered their own IXmlSchemaCache implementation before calling
    // AddXmlValidation(), that registration is preserved unchanged.
    services.TryAddSingleton<IXmlSchemaCache, XmlSchemaCache>();
    services.TryAddSingleton<IXmlSchemaValidator>(sp =>
    {
        var cache  = sp.GetRequiredService<IXmlSchemaCache>();
        var logger = sp.GetService<ILogger<XmlSchemaValidator>>(); // Optional — null if not registered

        // If a configure delegate was provided, the pre-built options instance is
        // used directly. Otherwise, options are resolved from the DI container via
        // IOptions<XmlValidationOptions> (supporting the Options pattern), falling
        // back to a default XmlValidationOptions instance if neither is registered.
        var effectiveOptions = configure is not null
            ? options
            : sp.GetService<IOptions<XmlValidationOptions>>()?.Value ?? options;

        return new XmlSchemaValidator(cache, effectiveOptions, logger);
    });

    return services;
}
```

### Key Design Decisions

1. **`TryAddSingleton` instead of `AddSingleton`:** The registration is idempotent. If `IXmlSchemaCache` or `IXmlSchemaValidator` is already registered in the container (e.g., a custom decorator or stub for testing), the existing registration takes precedence. This enables consumers to override the default implementations without risk of double-registration conflicts.

2. **`IOptions<XmlValidationOptions>` fallback:** When `configure` is `null` (i.e., `AddXmlValidation()` is called without arguments), the factory resolves `IOptions<XmlValidationOptions>` from the DI container first, enabling the standard ASP.NET Core Options pattern (`services.Configure<XmlValidationOptions>(...)`). If neither the delegate nor the Options registration is present, a default `XmlValidationOptions` instance is used.

## Consequences

### Positive
- Schemas are compiled once and reused across all incoming HTTP requests and background workers.
- Guarantees $O(1)$ schema lookup with zero redundant compilation overhead.
- Simplifies DI setup with a single extension method call.
- Idempotent registration via `TryAddSingleton` enables safe composition with custom implementations (decorators, mocks).
- Supports the standard ASP.NET Core `IOptions<T>` pattern for configuration.

### Negative
- `XmlSchemaCache` implementations must guarantee thread-safety for concurrent lookups.
- Because `TryAddSingleton` is used, calling `AddXmlValidation()` after registering a conflicting implementation will silently skip the library's default — callers must be aware of registration order when overriding.
