// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates validator customization and extensibility through decorator patterns.
/// </summary>
public static class Level08CustomizationAndExtensibility
{
    private const string TargetNamespace = "https://ericksonlopez.dev/schemas/orders";

    private const string OrderXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="https://ericksonlopez.dev/schemas/orders"
                   xmlns="https://ericksonlopez.dev/schemas/orders"
                   elementFormDefault="qualified">
          <xs:element name="Order">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="OrderId" type="xs:string" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    private const string ValidXml = """
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ORD-CUSTOM-001</OrderId>
        </Order>
        """;

    /// <summary>
    /// Executes the customization and decorator showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 08: CUSTOMIZATION AND EXTENSIBILITY VIA DECORATORS");
        Console.WriteLine("================================================================================\n");

        var services = new ServiceCollection();

        // 1. Library base registration
        services.AddXmlValidation();

        // 2. Decorating IXmlSchemaValidator to incorporate auditing and telemetry (100% Trim-Safe / AOT-Friendly)
        services.AddSingleton<IXmlSchemaValidator>(sp =>
        {
            var innerCache = sp.GetRequiredService<IXmlSchemaCache>();
            var inner = new XmlSchemaValidator(innerCache);
            return new AuditingXmlSchemaValidator(inner);
        });

        using var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IXmlSchemaCache>();
        cache.RegisterSchema(TargetNamespace, OrderXsd);

        var validator = sp.GetRequiredService<IXmlSchemaValidator>();

        Console.WriteLine($"[1] Executing validation through decorator '{validator.GetType().Name}':");
        var result = validator.Validate(ValidXml, TargetNamespace);
        Console.WriteLine($"    Result: IsSuccess={result.IsSuccess}");

        using var memStream = new MemoryStream(Encoding.UTF8.GetBytes(ValidXml));
        var asyncResult = await validator.ValidateAsync(memStream, TargetNamespace);
        Console.WriteLine($"    Async result: IsSuccess={asyncResult.IsSuccess}");

        // 2. Demonstrating custom implementation of IXmlSchemaCache
        Console.WriteLine("\n[2] Demonstrating custom implementation of IXmlSchemaCache (AuditingXmlSchemaCache):");
        var baseCache = new XmlSchemaCache();
        var auditingCache = new AuditingXmlSchemaCache(baseCache);

        // RegisterSchema (string overload)
        auditingCache.RegisterSchema(TargetNamespace, OrderXsd);
        Console.WriteLine($"    AuditingCache Count: {auditingCache.Count}, Registrations logged: {auditingCache.RegistrationCount}");

        // ContainsSchema
        var contains = auditingCache.ContainsSchema(TargetNamespace);
        Console.WriteLine($"    ContainsSchema('{TargetNamespace}'): {contains}");

        // IsRootElementDeclared
        var isDeclared = auditingCache.IsRootElementDeclared(TargetNamespace, "Order", TargetNamespace);
        Console.WriteLine($"    IsRootElementDeclared('Order', '{TargetNamespace}'): {isDeclared}");

        // RegisterSchema (Stream overload) via decorator
        using var xsdStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(OrderXsd));
        auditingCache.RegisterSchema(TargetNamespace, xsdStream);
        Console.WriteLine($"    After RegisterSchema(Stream): Registrations logged: {auditingCache.RegistrationCount}");

        // RegisterSchema (ReadOnlySpan<byte> overload) via decorator
        ReadOnlySpan<byte> xsdBytes = System.Text.Encoding.UTF8.GetBytes(OrderXsd);
        auditingCache.RegisterSchema(TargetNamespace, xsdBytes);
        Console.WriteLine($"    After RegisterSchema(Span<byte>): Registrations logged: {auditingCache.RegistrationCount}");

        // Clear() — demonstrates IXmlSchemaCache.Clear() propagation through the decorator
        Console.WriteLine("\n[3] Demonstrating IXmlSchemaCache.Clear() propagation through AuditingXmlSchemaCache decorator:");
        Console.WriteLine($"    Schemas before Clear(): {auditingCache.Count}");
        auditingCache.Clear();
        Console.WriteLine($"    Schemas after Clear(): {auditingCache.Count}");
        Console.WriteLine($"    ContainsSchema after Clear(): {auditingCache.ContainsSchema(TargetNamespace)}");

        Console.WriteLine("\n[4] Architectural Boundary Note (XML-API-001):");
        Console.WriteLine("    • IXmlSchemaCache defines the public contract for registration and inspection.");
        Console.WriteLine("    • XmlSchemaValidator internally requires IXmlSchemaSetProvider to retrieve compiled XmlSchemaSets.");
        Console.WriteLine("    • This encapsulation prevents external consumers from mutating the precompiled schema sets.");
        Console.WriteLine("    • AddXmlValidation() (no-arg overload) was used in [1] — AddXmlValidation follows the IServiceCollection");
        Console.WriteLine("      TryAdd pattern, so subsequent service registration can override the defaults (decorator pattern above).");

        Console.WriteLine("\n✔ Level 08 completed successfully.\n");
    }

    /// <summary>
    /// Demonstrative custom implementation of <see cref="IXmlSchemaCache"/> for telemetry and registration auditing.
    /// Encapsulated privately inside Level08 to enforce One-Type-Per-File.
    /// </summary>
    private sealed class AuditingXmlSchemaCache : IXmlSchemaCache
    {
        private readonly IXmlSchemaCache _inner;
        public int RegistrationCount { get; private set; }

        public AuditingXmlSchemaCache(IXmlSchemaCache inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public void RegisterSchema(string targetNamespace, string xsdContent)
        {
            RegistrationCount++;
            Console.WriteLine($"    [CacheAudit] RegisterSchema(string) for '{targetNamespace}' (Total registrations: {RegistrationCount})");
            _inner.RegisterSchema(targetNamespace, xsdContent);
        }

        public void RegisterSchema(string targetNamespace, Stream xsdStream)
        {
            RegistrationCount++;
            Console.WriteLine($"    [CacheAudit] RegisterSchema(Stream) for '{targetNamespace}' (Total registrations: {RegistrationCount})");
            _inner.RegisterSchema(targetNamespace, xsdStream);
        }

        public void RegisterSchema(string targetNamespace, ReadOnlySpan<byte> utf8Xsd)
        {
            RegistrationCount++;
            Console.WriteLine($"    [CacheAudit] RegisterSchema(Span) for '{targetNamespace}' (Total registrations: {RegistrationCount})");
            _inner.RegisterSchema(targetNamespace, utf8Xsd);
        }

        public bool ContainsSchema(string targetNamespace) => _inner.ContainsSchema(targetNamespace);

        public int Count => _inner.Count;

        public bool IsRootElementDeclared(string targetNamespace, string localName, string namespaceUri) =>
            _inner.IsRootElementDeclared(targetNamespace, localName, namespaceUri);

        public void Clear()
        {
            Console.WriteLine("    [CacheAudit] Clear() invoked on schema cache.");
            _inner.Clear();
        }
    }

    /// <summary>
    /// Nested decorator for <see cref="IXmlSchemaValidator"/> to measure and log schema validation duration.
    /// Encapsulated privately inside Level08 to enforce One-Type-Per-File.
    /// </summary>
    private sealed class AuditingXmlSchemaValidator : IXmlSchemaValidator
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
            Console.WriteLine($"    [Audit] Validate(string) took {sw.Elapsed.TotalMicroseconds:F1} µs. Success={result.IsSuccess}");
            return result;
        }

        public Result<bool> Validate(Stream xmlStream, string targetNamespace)
        {
            var sw = Stopwatch.StartNew();
            var result = _inner.Validate(xmlStream, targetNamespace);
            sw.Stop();
            Console.WriteLine($"    [Audit] Validate(Stream) took {sw.Elapsed.TotalMicroseconds:F1} µs. Success={result.IsSuccess}");
            return result;
        }

        public Result<bool> Validate(ReadOnlySpan<byte> utf8Xml, string targetNamespace)
        {
            var sw = Stopwatch.StartNew();
            var result = _inner.Validate(utf8Xml, targetNamespace);
            sw.Stop();
            Console.WriteLine($"    [Audit] Validate(Span) took {sw.Elapsed.TotalMicroseconds:F1} µs. Success={result.IsSuccess}");
            return result;
        }

        public async Task<Result<bool>> ValidateAsync(Stream xmlStream, string targetNamespace, CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();
            var result = await _inner.ValidateAsync(xmlStream, targetNamespace, cancellationToken);
            sw.Stop();
            Console.WriteLine($"    [Audit] ValidateAsync(Stream) took {sw.Elapsed.TotalMicroseconds:F1} µs. Success={result.IsSuccess}");
            return result;
        }
    }
}
