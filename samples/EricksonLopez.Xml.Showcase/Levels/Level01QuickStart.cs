// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates minimal standalone XML schema validation and caching without dependency injection.
/// </summary>
public static class Level01QuickStart
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
                <xs:element name="CustomerId" type="xs:string" />
                <xs:element name="Amount" type="xs:decimal" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    private const string ValidOrderXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ORD-2026-001</OrderId>
          <CustomerId>CUST-99</CustomerId>
          <Amount>150.50</Amount>
        </Order>
        """;

    private const string InvalidOrderXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ORD-2026-001</OrderId>
          <CustomerId>CUST-99</CustomerId>
          <Amount>NOT_A_DECIMAL</Amount>
        </Order>
        """;

    /// <summary>
    /// Executes the quick start showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 01: QUICK START (STANDALONE WITHOUT DI CONTAINER)");
        Console.WriteLine("================================================================================\n");

        // 1. Instantiating the precompiled schema cache
        Console.WriteLine("[1] Instantiating XmlSchemaCache...");
        var schemaCache = new XmlSchemaCache();
        Console.WriteLine($"    Cache initialized. Initial element count: {schemaCache.Count}");

        // 2. Registering and precompiling the XSD schema from string
        Console.WriteLine($"[2] Registering schema for target namespace: '{TargetNamespace}' (string overload)...");
        schemaCache.RegisterSchema(TargetNamespace, OrderXsd);
        Console.WriteLine($"    Schema compiled and cached. ContainsSchema: {schemaCache.ContainsSchema(TargetNamespace)}");
        Console.WriteLine($"    Total schemas registered: {schemaCache.Count}");

        // Demonstrating IXmlSchemaCache.IsRootElementDeclared
        var isOrderDeclared = schemaCache.IsRootElementDeclared(TargetNamespace, "Order", TargetNamespace);
        Console.WriteLine($"    IsRootElementDeclared('Order'): {isOrderDeclared}");

        // Demonstrating IXmlSchemaCache.RegisterSchema(string, Stream) overload
        Console.WriteLine("\n[2b] Demonstrating RegisterSchema(string, Stream) — stream registration overload:");
        using (var xsdStream = new MemoryStream(Encoding.UTF8.GetBytes(OrderXsd)))
        {
            schemaCache.RegisterSchema(TargetNamespace, xsdStream); // Atomic replacement
            Console.WriteLine($"    ✔ Schema re-registered from Stream. ContainsSchema: {schemaCache.ContainsSchema(TargetNamespace)}");
        }

        // 3. Creating the validator using the basic constructor
        Console.WriteLine("\n[3] Instantiating XmlSchemaValidator with registered cache...");
        var validator = new XmlSchemaValidator(schemaCache);

        // 4. Validating a conforming XML document
        Console.WriteLine("\n[4] Validating conforming XML document:");
        var validResult = validator.Validate(ValidOrderXml, TargetNamespace);
        if (validResult.IsSuccess)
        {
            Console.WriteLine($"    ✔ Success: Document is valid according to the XSD schema. (Value: {validResult.Value})");
        }
        else
        {
            Console.WriteLine($"    ❌ Unexpected failure: {validResult.Error.Description}");
        }

        // 5. Validating an XML document with a data type violation
        Console.WriteLine("\n[5] Validating non-conforming XML document (invalid xs:decimal data type):");
        var invalidResult = validator.Validate(InvalidOrderXml, TargetNamespace);
        if (invalidResult.IsFailure)
        {
            Console.WriteLine("    ✔ Expected outcome: Validation failed with structured error.");
            Console.WriteLine($"      Error Code: {invalidResult.Error.Code}");
            Console.WriteLine($"      Violation Detail: {invalidResult.Error.Description}");
        }

        // 6. Validating against an unregistered namespace
        Console.WriteLine("\n[6] Validating against an unregistered namespace ('https://unknown.org'):");
        var unregisteredResult = validator.Validate(ValidOrderXml, "https://unknown.org");
        if (unregisteredResult.IsFailure)
        {
            Console.WriteLine($"      Error Code: {unregisteredResult.Error.Code} ({unregisteredResult.Error.Type})");
            Console.WriteLine($"      Description: {unregisteredResult.Error.Description}");
        }

        // 7. Security Invariant: TryGetSchemaSet is an internal abstraction on IXmlSchemaSetProvider
        // to protect the precompiled XmlSchemaSet from external mutation. The public cache encapsulates it securely.

        // 8. Demonstrating RegisterSchema(ReadOnlySpan<byte>) — high-performance UTF-8 byte overload
        Console.WriteLine("\n[8] Demonstrating IXmlSchemaCache.RegisterSchema(ReadOnlySpan<byte>) — registration from UTF-8 buffer:");
        Console.WriteLine("    (Useful in network pipelines: validates buffers directly without allocating intermediate strings)");
        var xsdBytes = Encoding.UTF8.GetBytes(OrderXsd);
        ReadOnlySpan<byte> utf8Span = xsdBytes;
        schemaCache.RegisterSchema(TargetNamespace, utf8Span); // Re-registers (atomic idempotent update)
        Console.WriteLine($"    ✔ Schema registered from ReadOnlySpan<byte>. ContainsSchema: {schemaCache.ContainsSchema(TargetNamespace)}");
        Console.WriteLine($"    Total schemas in cache: {schemaCache.Count}");

        // 9. Demonstrating Clear() — atomic cache reset
        Console.WriteLine("\n[9] Demonstrating IXmlSchemaCache.Clear() — atomic cache clearing:");
        Console.WriteLine($"    Schemas before Clear(): {schemaCache.Count}");
        schemaCache.Clear();
        Console.WriteLine($"    ✔ Clear() executed. Schemas after: {schemaCache.Count}");
        Console.WriteLine($"    ContainsSchema after Clear(): {schemaCache.ContainsSchema(TargetNamespace)}");

        Console.WriteLine("\n✔ Level 01 completed successfully.\n");
        return Task.CompletedTask;
    }
}
