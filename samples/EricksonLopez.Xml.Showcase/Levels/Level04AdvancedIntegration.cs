// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates input modalities including directory scanning, streams, byte spans, and async cancellation.
/// </summary>
public static class Level04AdvancedIntegration
{
    private const string TargetNamespace = "https://ericksonlopez.dev/schemas/orders";

    private const string SampleXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ORD-999</OrderId>
          <CustomerId>CUST-100</CustomerId>
          <OrderDate>2026-09-02T12:00:00Z</OrderDate>
          <Items>
            <Item>
              <Sku>SKU-001</Sku>
              <Quantity>5</Quantity>
              <UnitPrice>19.99</UnitPrice>
            </Item>
          </Items>
        </Order>
        """;

    /// <summary>
    /// Executes the advanced integration showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 04: ADVANCED INTEGRATION AND INPUT MODALITIES");
        Console.WriteLine("================================================================================\n");

        var cache = new XmlSchemaCache();
        var validator = new XmlSchemaValidator(cache);

        // 1. Bulk schema loading from directory
        var schemasDir = Path.Combine(AppContext.BaseDirectory, "Schemas");
        Console.WriteLine($"[1] Scanning and bulk registering schemas from: {schemasDir}");
        var registeredCount = cache.RegisterSchemasFromDirectory(schemasDir, "*.xsd");
        Console.WriteLine($"    ✔ Schemas discovered and compiled successfully: {registeredCount}");

        // 2. Modality 1: In-memory string
        Console.WriteLine("\n[2] Modality 1: Validation from in-memory string:");
        var resultString = validator.Validate(SampleXml, TargetNamespace);
        Console.WriteLine($"    String result: IsSuccess={resultString.IsSuccess}");

        // 3. Modality 2: Synchronous stream
        Console.WriteLine("\n[3] Modality 2: Validation from synchronous Stream:");
        byte[] xmlBytes = Encoding.UTF8.GetBytes(SampleXml);
        using (var stream = new MemoryStream(xmlBytes))
        {
            var resultStream = validator.Validate(stream, TargetNamespace);
            Console.WriteLine($"    Stream result: IsSuccess={resultStream.IsSuccess}");
        }

        // 4. Modality 3: ReadOnlySpan<byte> (UTF-8 zero-allocation)
        Console.WriteLine("\n[4] Modality 3: High-performance validation over ReadOnlySpan<byte> (UTF-8):");
        ReadOnlySpan<byte> span = xmlBytes;
        var resultSpan = validator.Validate(span, TargetNamespace);
        Console.WriteLine($"    Span result: IsSuccess={resultSpan.IsSuccess}");

        // 5. Modality 4: Asynchronous Stream validation with CancellationToken
        Console.WriteLine("\n[5] Modality 4: Asynchronous validation with CancellationToken:");
        using (var asyncStream = new MemoryStream(xmlBytes))
        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
        {
            var resultAsync = await validator.ValidateAsync(asyncStream, TargetNamespace, cts.Token);
            Console.WriteLine($"    Async result: IsSuccess={resultAsync.IsSuccess}");
        }

        Console.WriteLine("\n✔ Level 04 completed successfully.\n");
    }
}
