// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates the foundational concepts and security architecture of XML schema validation.
/// </summary>
public static class Level00Conceptual
{
    /// <summary>
    /// Executes the conceptual foundations showcase asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 00: CONCEPTUAL FOUNDATIONS AND SECURITY ARCHITECTURE");
        Console.WriteLine("================================================================================\n");

        Console.WriteLine("1. WHAT IS ERICKSONLOPEZ.XML.VALIDATION?");
        Console.WriteLine("   It is a high-performance XSD schema validation engine for .NET (net8.0+),");
        Console.WriteLine("   designed with strict Anti-XXE security by default, thread-safe in-memory caching");
        Console.WriteLine("   of precompiled XmlSchemaSet instances, typed errors via Result<T>, and integration");
        Console.WriteLine("   with Microsoft Dependency Injection and zero-allocation structured logging.\n");

        Console.WriteLine("2. WHAT PROBLEMS DOES IT SOLVE?");
        Console.WriteLine("   a) XXE (XML External Entity) Vulnerabilities:");
        Console.WriteLine("      By default, many XmlReader configurations across the .NET ecosystem leave");
        Console.WriteLine("      doors open to external DTD resolution and Billion Laughs attacks.");
        Console.WriteLine("      EricksonLopez.Xml.Validation ENFORCES DtdProcessing.Prohibit and XmlResolver = null");
        Console.WriteLine("      on every code path without exception.\n");

        Console.WriteLine("   b) Repetitive Schema Compilation Overhead:");
        Console.WriteLine("      Compiling an XmlSchemaSet on every web request is extremely costly (GC pressure");
        Console.WriteLine("      and 10-100x extra latency). The library introduces IXmlSchemaCache backed by");
        Console.WriteLine("      ConcurrentDictionary indexed by targetNamespace for one-time compilation (O(1)).\n");

        Console.WriteLine("   c) Exception Overhead for Control Flow:");
        Console.WriteLine("      The .NET BCL throws XmlSchemaValidationException when a document violates the XSD.");
        Console.WriteLine("      Schema violations are not runtime anomalies; they are expected validation outcomes.");
        Console.WriteLine("      This library encapsulates results in Result<bool> with canonical error codes:\n");
        Console.WriteLine("      - 'XmlValidation.SchemaNotRegistered' (Error.NotFound)");
        Console.WriteLine("      - 'XmlValidation.SchemaViolation'     (Error.Validation)");
        Console.WriteLine("      - 'XmlValidation.XmlMalformed'        (Error.Validation)\n");

        Console.WriteLine("3. ARCHITECTURAL COMPARISON:");
        Console.WriteLine("   ┌──────────────────────────────┬────────────────────────┬─────────────────────────────┐");
        Console.WriteLine("   │ Feature                      │ Direct .NET BCL        │ EricksonLopez.Xml.Validation│");
        Console.WriteLine("   ├──────────────────────────────┼────────────────────────┼─────────────────────────────┤");
        Console.WriteLine("   │ Anti-XXE by Default          │ Manual (easy to miss)  │ Enforced (cannot disable)   │");
        Console.WriteLine("   │ Error Handling               │ Try/Catch Exceptions   │ Result<bool> (Railway-Orient)│");
        Console.WriteLine("   │ Schema Caching               │ Manual / Ad-hoc        │ IXmlSchemaCache Thread-Safe │");
        Console.WriteLine("   │ Dependency Injection         │ Manual Boilerplate     │ services.AddXmlValidation() │");
        Console.WriteLine("   │ Structured Logging           │ Not Integrated         │ [LoggerMessage] Zero-Alloc  │");
        Console.WriteLine("   │ Native AOT Support           │ Depends on Config      │ Verified IsAotCompatible    │");
        Console.WriteLine("   └──────────────────────────────┴────────────────────────┴─────────────────────────────┘\n");

        Console.WriteLine("4. DESIGN INVARIANTS:");
        Console.WriteLine("   • Invariant 1: Uncompromising Anti-XXE. No parameter exists to re-enable DTDs.");
        Console.WriteLine("   • Invariant 2: targetNamespace as canonical 1:1 cache key.");
        Console.WriteLine("   • Invariant 3: Configurable warnings (IncludeWarnings, TreatWarningsAsErrors).");
        Console.WriteLine("   • Invariant 4: Zero allocations in logging via [LoggerMessage].\n");

        Console.WriteLine("✔ Level 00 completed successfully.\n");
        return Task.CompletedTask;
    }
}
