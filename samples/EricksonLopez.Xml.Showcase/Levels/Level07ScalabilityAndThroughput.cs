// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates throughput benchmarks and memory efficiency using UTF-8 byte spans and precompiled schemas.
/// </summary>
public static class Level07ScalabilityAndThroughput
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
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    private const string SampleXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ORD-PERF-01</OrderId>
          <CustomerId>CUST-PERF-01</CustomerId>
        </Order>
        """;

    /// <summary>
    /// Executes the scalability and throughput showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 07: SCALABILITY, THROUGHPUT, AND MEMORY EFFICIENCY");
        Console.WriteLine("================================================================================\n");

        const int iterations = 1000;
        Console.WriteLine($"[1] Comparative Benchmark ({iterations} iterations):");

        // Traditional BCL approach: Ad-hoc compilation of XmlSchemaSet on every request
        var swUncached = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var reader = new StringReader(OrderXsd);
            using var xmlReader = XmlReader.Create(reader);
            var set = new System.Xml.Schema.XmlSchemaSet { XmlResolver = null };
            set.Add(TargetNamespace, xmlReader);
            set.Compile(); // Massive hot-path compilation overhead
        }
        swUncached.Stop();
        Console.WriteLine($"    • Ad-hoc compilation (No Cache):      {swUncached.ElapsedMilliseconds,6} ms (Total)");

        // EricksonLopez approach: Precompiled IXmlSchemaCache
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, OrderXsd);
        var validator = new XmlSchemaValidator(cache);

        var swCached = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = validator.Validate(SampleXml, TargetNamespace);
        }
        swCached.Stop();
        Console.WriteLine($"    • Precompiled with IXmlSchemaCache:   {swCached.ElapsedMilliseconds,6} ms (Total)");

        double speedup = (double)swUncached.ElapsedMilliseconds / Math.Max(1, swCached.ElapsedMilliseconds);
        Console.WriteLine($"    ✔ Speedup Factor: ~{speedup:F1}x faster via precompiled schema cache.\n");

        // 2. Memory efficiency: UTF-8 ReadOnlySpan<byte>
        Console.WriteLine("[2] Throughput Demonstration with ReadOnlySpan<byte> (Native UTF-8 buffer):");
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(SampleXml);

        var swSpan = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ReadOnlySpan<byte> span = utf8Bytes;
            _ = validator.Validate(span, TargetNamespace);
        }
        swSpan.Stop();
        Console.WriteLine($"    • {iterations} validations on Span<byte>: {swSpan.ElapsedMilliseconds} ms");
        Console.WriteLine("      (Enables processing network and socket buffers without allocating strings on the heap/LOH)");

        Console.WriteLine("\n✔ Level 07 completed successfully.\n");
        return Task.CompletedTask;
    }
}
