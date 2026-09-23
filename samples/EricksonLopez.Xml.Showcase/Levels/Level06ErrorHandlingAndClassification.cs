// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates failure taxonomy, structured error classification, Anti-XXE defense mechanisms,
/// empty-payload detection, and the TreatWarningsAsErrors pathway.
/// </summary>
public static class Level06ErrorHandlingAndClassification
{
    private const string TargetNamespace = "https://ericksonlopez.dev/schemas/orders";
    private const string WarningsNamespace = "https://ericksonlopez.dev/schemas/warnings";

    // Classic XXE injection payload with external DTD and entity expansion
    private const string XxePayloadXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <!DOCTYPE Order [
          <!ELEMENT Order ANY >
          <!ENTITY xxe SYSTEM "file:///etc/passwd" >]>
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>&xxe;</OrderId>
          <CustomerId>HACKER</CustomerId>
          <OrderDate>2026-09-02T12:00:00Z</OrderDate>
        </Order>
        """;

    // Syntactically malformed XML (unclosed element tag)
    private const string MalformedXml = """
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ORD-123<OrderId> <!-- Malformed closing tag -->
        </Order>
        """;

    // XML document violating schema constraint (Quantity minInclusive=1, passing 0)
    private const string SchemaViolationXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ORD-123</OrderId>
          <CustomerId>CUST-1</CustomerId>
          <OrderDate>2026-09-02T12:00:00Z</OrderDate>
          <Items>
            <Item>
              <Sku>SKU-1</Sku>
              <!-- Quantity minInclusive is 1, passing 0 -->
              <Quantity>0</Quantity>
              <UnitPrice>10.00</UnitPrice>
            </Item>
          </Items>
        </Order>
        """;

    // XML conforming to warnings.xsd — contains xs:any lax, which can emit schema warnings
    private const string ExtensibleMessageXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <ExtensibleMessage xmlns="https://ericksonlopez.dev/schemas/warnings">
          <Header>WarningTest</Header>
          <Payload>
            <AnyElement xmlns="https://custom-extension.example">ExtensionData</AnyElement>
          </Payload>
        </ExtensibleMessage>
        """;

    /// <summary>
    /// Executes the error handling and security defenses showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 06: ERROR TAXONOMY AND ANTI-XXE SECURITY DEFENSES (OWASP TOP 10)");
        Console.WriteLine("================================================================================\n");

        var cache = new XmlSchemaCache();
        var schemasDir = Path.Combine(AppContext.BaseDirectory, "Schemas");
        cache.RegisterSchemasFromDirectory(schemasDir, "*.xsd");

        var validator = new XmlSchemaValidator(cache);

        // ── Error 1: XmlValidation.SchemaNotRegistered ────────────────────────────
        Console.WriteLine("[1] Error: Unregistered schema ('XmlValidation.SchemaNotRegistered')");
        var notFoundResult = validator.Validate("<Root />", "https://unregistered-namespace.org");
        Console.WriteLine($"    • IsFailure: {notFoundResult.IsFailure}");
        Console.WriteLine($"    • Error Type: {notFoundResult.Error.Type}");
        Console.WriteLine($"    • Code: {notFoundResult.Error.Code}");
        Console.WriteLine($"    • Description: {notFoundResult.Error.Description}\n");

        // ── Error 2: XmlValidation.SchemaViolation ────────────────────────────────
        Console.WriteLine("[2] Error: Schema constraint violation ('XmlValidation.SchemaViolation')");
        var violationResult = validator.Validate(SchemaViolationXml, TargetNamespace);
        Console.WriteLine($"    • IsFailure: {violationResult.IsFailure}");
        Console.WriteLine($"    • Code: {violationResult.Error.Code}");
        Console.WriteLine($"    • Line/Column Detail: {violationResult.Error.Description}\n");

        // ── Error 3: XmlValidation.XmlMalformed (syntactically broken document) ──
        Console.WriteLine("[3] Error: Syntactically malformed XML ('XmlValidation.XmlMalformed')");
        var malformedResult = validator.Validate(MalformedXml, TargetNamespace);
        Console.WriteLine($"    • IsFailure: {malformedResult.IsFailure}");
        Console.WriteLine($"    • Code: {malformedResult.Error.Code}");
        Console.WriteLine($"    • Detail: {malformedResult.Error.Description}\n");

        // ── Error 4: XmlValidation.XmlMalformed (empty UTF-8 span) ───────────────
        Console.WriteLine("[4] Error: Empty UTF-8 payload ('XmlValidation.XmlMalformed' — ReadOnlySpan<byte>.Empty path)");
        Console.WriteLine("    Validate(ReadOnlySpan<byte>) returns XmlMalformed immediately when span is empty.");
        Console.WriteLine("    This prevents forwarding empty buffers from network pipelines to the XML parser.");
        ReadOnlySpan<byte> emptySpan = ReadOnlySpan<byte>.Empty;
        var emptyResult = validator.Validate(emptySpan, TargetNamespace);
        Console.WriteLine($"    • IsFailure: {emptyResult.IsFailure}");
        Console.WriteLine($"    • Code: {emptyResult.Error.Code}");
        Console.WriteLine($"    • Detail: {emptyResult.Error.Description}\n");

        // ── Security: XXE Injection (DTD + external entity) ──────────────────────
        Console.WriteLine("[5] Security: XXE Injection Attempt with DTD Definition");
        var xxeResult = validator.Validate(XxePayloadXml, TargetNamespace);
        Console.WriteLine($"    • Attack Neutralized: {xxeResult.IsFailure}");
        Console.WriteLine($"    • Assigned Code: {xxeResult.Error.Code}");
        Console.WriteLine($"    • Security Message: {xxeResult.Error.Description}");
        Console.WriteLine("      (DtdProcessing.Prohibit immediately blocked DTD processing prior to entity resolution)\n");

        // ── DoS Limit: MaxCharactersInDocument ───────────────────────────────────
        Console.WriteLine("[6] Demonstrating DoS Defense — MaxCharactersInDocument + MaxErrors:");
        var strictOptions = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = false,
            MaxCharactersInDocument = 150, // Small limit for demonstration
            MaxErrors = 1
        };
        var strictValidator = new XmlSchemaValidator(cache, strictOptions);
        Console.WriteLine($"    • MaxCharactersInDocument={strictOptions.MaxCharactersInDocument}, MaxErrors={strictOptions.MaxErrors}");

        var oversizedXml = SchemaViolationXml; // Larger than 150 characters
        var dosResult = strictValidator.Validate(oversizedXml, TargetNamespace);
        Console.WriteLine($"    • DoS Payload Blocked: {dosResult.IsFailure}");
        Console.WriteLine($"      Assigned Code: {dosResult.Error.Code}");
        Console.WriteLine($"      Defense Detail: {dosResult.Error.Description}");

        // ── TreatWarningsAsErrors pathway ─────────────────────────────────────────
        Console.WriteLine("\n[7] TreatWarningsAsErrors = true — using warnings.xsd (xs:any lax model):");
        Console.WriteLine("    The warnings.xsd schema uses xs:any with processContents='lax',");
        Console.WriteLine("    which may emit schema warnings on certain validators. This step configures");
        Console.WriteLine("    TreatWarningsAsErrors=true and IncludeWarnings (implied) to capture them.");

        // With TreatWarningsAsErrors=true, IncludeWarnings automatically returns true (design invariant)
        var warningOptions = new XmlValidationOptions
        {
            TreatWarningsAsErrors = true
        };
        Console.WriteLine($"    • TreatWarningsAsErrors={warningOptions.TreatWarningsAsErrors}");
        Console.WriteLine($"    • IncludeWarnings (read): {warningOptions.IncludeWarnings} (auto-implied — prevents silent warning drops)");
        var warningValidator = new XmlSchemaValidator(cache, warningOptions);

        var warningResult = warningValidator.Validate(ExtensibleMessageXml, WarningsNamespace);

        // Note: Whether a warning is actually emitted depends on the .NET XSD engine behavior
        // for xs:any lax. Either outcome is valid and informative.
        if (warningResult.IsFailure)
        {
            Console.WriteLine($"    ✔ Warning captured and treated as error. Code: {warningResult.Error.Code}");
            Console.WriteLine($"      Detail: {warningResult.Error.Description}");
        }
        else
        {
            Console.WriteLine("    ✔ Document valid — .NET XSD engine did not emit a warning for xs:any lax in this context.");
            Console.WriteLine("      The TreatWarningsAsErrors pathway is still active and would fail if a warning were emitted.");
        }

        // Demonstrate IncludeWarnings without TreatWarningsAsErrors (warnings captured but not fatal)
        Console.WriteLine("\n[8] IncludeWarnings=true (without TreatWarningsAsErrors): warnings are informational only:");
        var infoOptions = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = false
        };
        Console.WriteLine($"    • IncludeWarnings={infoOptions.IncludeWarnings}, TreatWarningsAsErrors={infoOptions.TreatWarningsAsErrors}");
        Console.WriteLine("""
            Schema warnings are captured in the Error.Description with '[Warning]' prefix,
              but IsFailure remains false — document is still considered valid.
            """);

        // ── File System Error: RegisterSchemaFile (FileNotFoundException) ─────────
        Console.WriteLine("\n[9] File System Errors — RegisterSchemaFile + RegisterSchemasFromDirectory:");
        Console.WriteLine("    XmlSchemaCacheExtensions throw typed BCL exceptions for missing paths (not Result<T>).");
        Console.WriteLine("    These exceptions occur at schema registration time (startup), not at validation time.");

        // FileNotFoundException from RegisterSchemaFile
        Console.WriteLine("\n    [9a] RegisterSchemaFile: FileNotFoundException for a non-existent XSD file:");
        try
        {
            cache.RegisterSchemaFile(TargetNamespace, Path.Combine(AppContext.BaseDirectory, "Schemas", "nonexistent.xsd"));
            Console.WriteLine("    ❌ Error: FileNotFoundException was expected.");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"    ✔ FileNotFoundException caught: {ex.Message}");
        }

        // DirectoryNotFoundException from RegisterSchemasFromDirectory
        Console.WriteLine("\n    [9b] RegisterSchemasFromDirectory: DirectoryNotFoundException for a non-existent directory:");
        try
        {
            cache.RegisterSchemasFromDirectory(Path.Combine(AppContext.BaseDirectory, "NonExistentSchemaDir"), "*.xsd");
            Console.WriteLine("    ❌ Error: DirectoryNotFoundException was expected.");
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"    ✔ DirectoryNotFoundException caught: {ex.Message}");
        }

        Console.WriteLine("\n    Note: These exceptions are thrown synchronously at registration time and should be");
        Console.WriteLine("          handled in application startup (e.g., IHostedService.StartAsync) with fast-fail");
        Console.WriteLine("          behavior to prevent deploying a service with incomplete schema catalogs.\n");

        Console.WriteLine("\n✔ Level 06 completed successfully.\n");
        return Task.CompletedTask;
    }
}
