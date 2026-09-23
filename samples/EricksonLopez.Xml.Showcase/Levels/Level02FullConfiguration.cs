// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Threading.Tasks;
using EricksonLopez.Xml.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates dependency injection integration, logging configuration, validation options tuning,
/// and the full configuration surface of XmlValidationOptions including all five configurable properties.
/// </summary>
public static class Level02FullConfiguration
{
    private const string TargetNamespace = "https://ericksonlopez.dev/schemas/appconfig";

    // Full ConfigXml matching the config.xsd on disk:
    //   Environment (string), LogLevel (string), TimeoutSeconds (int),
    //   FeatureToggles > EnableAntiXxe (bool), EnableCaching (bool)
    private const string ValidConfigXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <AppConfiguration xmlns="https://ericksonlopez.dev/schemas/appconfig">
          <Environment>Production</Environment>
          <LogLevel>Information</LogLevel>
          <TimeoutSeconds>30</TimeoutSeconds>
          <FeatureToggles>
            <EnableAntiXxe>true</EnableAntiXxe>
            <EnableCaching>true</EnableCaching>
          </FeatureToggles>
        </AppConfiguration>
        """;

    // ConfigXml deliberately missing mandatory LogLevel to produce a SchemaViolation
    private const string InvalidConfigXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <AppConfiguration xmlns="https://ericksonlopez.dev/schemas/appconfig">
          <Environment>Staging</Environment>
          <!-- Missing mandatory LogLevel, TimeoutSeconds, and FeatureToggles -->
        </AppConfiguration>
        """;

    /// <summary>
    /// Executes the full configuration showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 02: FULL CONFIGURATION AND DEPENDENCY INJECTION");
        Console.WriteLine("================================================================================\n");

        // ── Step 1 ────────────────────────────────────────────────────────────────
        // AddXmlValidation(Action<XmlValidationOptions>) — custom configuration delegate
        Console.WriteLine("[1] AddXmlValidation(Action<XmlValidationOptions>) — custom configuration delegate:");
        Console.WriteLine("    Registering IXmlSchemaCache + IXmlSchemaValidator as Singletons with custom options...");
        var services = new ServiceCollection();

        // Logging configuration to observe compile-time [LoggerMessage] delegates (EventIds 1001-1005)
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(opts =>
            {
                opts.IncludeScopes = false;
                opts.SingleLine = true;
                opts.TimestampFormat = "HH:mm:ss.fff ";
            });
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Registering validation services with custom options covering the complete configuration surface
        services.AddXmlValidation(options =>
        {
            options.IncludeWarnings = true;
            options.TreatWarningsAsErrors = false;
            options.MaxCharactersInDocument = 5_000_000; // Defense against XML DoS
            options.MaxErrors = 50;                     // Defense against memory exhaustion
            options.ProcessInlineSchema = false;        // Defense against inline schema poisoning
        });

        using var serviceProvider = services.BuildServiceProvider();

        // ── Step 2 ────────────────────────────────────────────────────────────────
        // Verifying service registrations and Singleton lifecycle guarantee
        Console.WriteLine("\n[2] Verifying Singleton lifecycle guarantee (same instance across DI scopes):");
        using (var scope1 = serviceProvider.CreateScope())
        using (var scope2 = serviceProvider.CreateScope())
        {
            var cache1 = scope1.ServiceProvider.GetRequiredService<IXmlSchemaCache>();
            var cache2 = scope2.ServiceProvider.GetRequiredService<IXmlSchemaCache>();
            var validator1 = scope1.ServiceProvider.GetRequiredService<IXmlSchemaValidator>();
            var validator2 = scope2.ServiceProvider.GetRequiredService<IXmlSchemaValidator>();

            var sameCache = ReferenceEquals(cache1, cache2);
            var sameValidator = ReferenceEquals(validator1, validator2);

            Console.WriteLine($"    • IXmlSchemaCache is Singleton across scopes: {sameCache}");
            Console.WriteLine($"    • IXmlSchemaValidator is Singleton across scopes: {sameValidator}");
        }

        // ── Step 3 ────────────────────────────────────────────────────────────────
        // Registering the config schema from disk using RegisterSchemaFile (XmlSchemaCacheExtensions)
        Console.WriteLine($"\n[3] Registering config.xsd from disk via RegisterSchemasFromDirectory...");
        var cache = serviceProvider.GetRequiredService<IXmlSchemaCache>();
        var schemasDir = Path.Combine(AppContext.BaseDirectory, "Schemas");
        cache.RegisterSchemaFile(TargetNamespace, Path.Combine(schemasDir, "config.xsd"));
        var hasRoot = cache.IsRootElementDeclared(TargetNamespace, "AppConfiguration", TargetNamespace);
        Console.WriteLine($"    Schema registered. Total schemas in cache: {cache.Count}, IsRootElementDeclared: {hasRoot}");

        // ── Step 4 ────────────────────────────────────────────────────────────────
        // Validation against the full on-disk config.xsd schema
        Console.WriteLine("\n[4] Validating conforming AppConfiguration document (all fields including LogLevel and FeatureToggles):");
        var validator = serviceProvider.GetRequiredService<IXmlSchemaValidator>();
        var validResult = validator.Validate(ValidConfigXml, TargetNamespace);
        Console.WriteLine($"    IsSuccess: {validResult.IsSuccess}, Value: {validResult.Value}");

        Console.WriteLine("\n[4b] Validating non-conforming AppConfiguration document (missing mandatory elements):");
        var invalidResult = validator.Validate(InvalidConfigXml, TargetNamespace);
        if (invalidResult.IsFailure)
        {
            Console.WriteLine($"    ✔ Expected failure. Code: {invalidResult.Error.Code}");
            Console.WriteLine($"    Detail: {invalidResult.Error.Description}");
        }

        // ── Step 5 ────────────────────────────────────────────────────────────────
        // AddXmlValidation() — no-argument overload using IOptions<XmlValidationOptions>
        Console.WriteLine("\n[5] AddXmlValidation() — no-argument overload with IOptions<XmlValidationOptions> pipeline:");
        Console.WriteLine("    When called without a delegate, options are resolved from the DI Options pipeline.");
        Console.WriteLine("    This allows external configuration via appsettings.json or configuration binders.");
        var servicesB = new ServiceCollection();
        servicesB.Configure<XmlValidationOptions>(opts =>
        {
            opts.IncludeWarnings = false;
            opts.MaxCharactersInDocument = 1_000_000;
            opts.MaxErrors = 25;
        });
        servicesB.AddXmlValidation(); // Uses IOptions<XmlValidationOptions> from the DI container
        servicesB.AddLogging(b => b.AddSimpleConsole(o => o.SingleLine = true).SetMinimumLevel(LogLevel.Warning));

        using var spB = servicesB.BuildServiceProvider();
        var optionsCacheB = spB.GetRequiredService<IXmlSchemaCache>();
        optionsCacheB.RegisterSchemaFile(TargetNamespace, Path.Combine(schemasDir, "config.xsd"));
        var validatorB = spB.GetRequiredService<IXmlSchemaValidator>();

        // Verify options were consumed from IOptions<XmlValidationOptions>
        var resolvedOptions = spB.GetRequiredService<IOptions<XmlValidationOptions>>().Value;
        Console.WriteLine($"    IOptions<XmlValidationOptions>: IncludeWarnings={resolvedOptions.IncludeWarnings}, MaxErrors={resolvedOptions.MaxErrors}");
        var resultB = validatorB.Validate(ValidConfigXml, TargetNamespace);
        Console.WriteLine($"    Validation result: IsSuccess={resultB.IsSuccess}");

        // ── Step 6 ────────────────────────────────────────────────────────────────
        // Complete XmlValidationOptions surface documentation
        Console.WriteLine("\n[6] Complete XmlValidationOptions configuration surface (5 properties):");
        Console.WriteLine("    • IncludeWarnings = true|false  : Captures XSD warnings with '[Warning]' prefix. Default: false.");
        Console.WriteLine("                                      If TreatWarningsAsErrors=true, always returns true (no silent drop).");
        Console.WriteLine("    • TreatWarningsAsErrors = true  : Any XSD schema warning becomes a validation failure. Default: false.");
        Console.WriteLine("    • MaxCharactersInDocument       : Ceiling on document size to mitigate XML DoS. Default: 10,000,000. 0=unlimited.");
        Console.WriteLine("    • MaxErrors = N                 : Max errors + warnings collected. Prevents memory exhaustion. Default: 100.");
        Console.WriteLine("    • ProcessInlineSchema = false   : Prohibit inline schemas by default (schema poisoning defense). Default: false.");
        Console.WriteLine("                                      Set true ONLY when processing documents with trusted inline schemas.");

        Console.WriteLine("\n✔ Level 02 completed successfully.\n");
        return Task.CompletedTask;
    }
}
