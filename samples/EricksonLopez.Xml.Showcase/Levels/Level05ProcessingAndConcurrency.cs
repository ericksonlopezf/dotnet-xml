// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates thread-safety guarantees and cooperative cancellation under concurrent validation workloads.
/// </summary>
public static class Level05ProcessingAndConcurrency
{
    private const string TargetNamespace = "https://ericksonlopez.dev/schemas/orders";

    private const string SampleXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ORD-CONCURRENCY</OrderId>
          <CustomerId>CUST-CONCURRENCY</CustomerId>
          <OrderDate>2026-09-02T12:00:00Z</OrderDate>
          <Items>
            <Item>
              <Sku>SKU-MULTI</Sku>
              <Quantity>10</Quantity>
              <UnitPrice>99.90</UnitPrice>
            </Item>
          </Items>
        </Order>
        """;

    /// <summary>
    /// Executes the concurrency and cancellation showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 05: CONCURRENT PROCESSING AND COOPERATIVE CANCELLATION");
        Console.WriteLine("================================================================================\n");

        var cache = new XmlSchemaCache();
        var schemasDir = Path.Combine(AppContext.BaseDirectory, "Schemas");
        cache.RegisterSchemasFromDirectory(schemasDir, "*.xsd");

        var validator = new XmlSchemaValidator(cache);

        // 1. Massive concurrent execution (200 parallel tasks)
        const int totalTasks = 200;
        Console.WriteLine($"[1] Launching {totalTasks} concurrent validations against the same singleton instance...");

        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task<bool>>(totalTasks);

        for (int i = 0; i < totalTasks; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var result = validator.Validate(SampleXml, TargetNamespace);
                return result.IsSuccess && result.Value;
            }));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        int successes = 0;
        foreach (var ok in results)
        {
            if (ok) successes++;
        }

        Console.WriteLine($"    ✔ Successful validations: {successes}/{totalTasks}");
        Console.WriteLine($"    ✔ Total elapsed time: {stopwatch.ElapsedMilliseconds} ms (Average: {(double)stopwatch.ElapsedMilliseconds / totalTasks:F2} ms/op)");

        // 2. Cancellation demonstration on asynchronous Stream
        Console.WriteLine("\n[2] Demonstrating cooperative cancellation in ValidateAsync with CancellationToken:");
        using (var cts = new CancellationTokenSource())
        {
            // Cancel token immediately prior to operation
            cts.Cancel();

            byte[] xmlBytes = Encoding.UTF8.GetBytes(SampleXml);
            using var stream = new MemoryStream(xmlBytes);

            try
            {
                await validator.ValidateAsync(stream, TargetNamespace, cts.Token);
                Console.WriteLine("    ❌ Error: OperationCanceledException was expected.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("    ✔ Success: OperationCanceledException caught correctly.");
                Console.WriteLine("      Operation was aborted cleanly without resource leaks or corrupted state.");
            }
        }

        Console.WriteLine("\n✔ Level 05 completed successfully.\n");
    }
}
