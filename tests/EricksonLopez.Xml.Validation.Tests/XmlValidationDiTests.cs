// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Linq;
using System.Text;
using AwesomeAssertions;
using EricksonLopez.Xml.Validation.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using static EricksonLopez.Xml.Validation.Tests.Common.XmlTestSamples;

namespace EricksonLopez.Xml.Validation.Tests;

/// <summary>
/// Verifies dependency injection registration, options configuration, and logging integration
/// for <see cref="XmlValidationServiceCollectionExtensions"/>.
/// </summary>
public sealed class XmlValidationDiTests
{
    // ─── Service Registration ─────────────────────────────────────────────────

    [Fact]
    public void AddXmlValidation_RegistersIXmlSchemaCache()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var cache = sp.GetService<IXmlSchemaCache>();

        cache.Should().NotBeNull();
    }

    [Fact]
    public void AddXmlValidation_RegistersIXmlSchemaValidator()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var validator = sp.GetService<IXmlSchemaValidator>();

        validator.Should().NotBeNull();
    }

    [Fact]
    public void AddXmlValidation_CacheIsSingleton()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var cache1 = sp.GetRequiredService<IXmlSchemaCache>();
        var cache2 = sp.GetRequiredService<IXmlSchemaCache>();

        // Singleton: same instance
        cache1.Should().BeSameAs(cache2);
    }

    [Fact]
    public void AddXmlValidation_ValidatorIsSingleton()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var validator1 = sp.GetRequiredService<IXmlSchemaValidator>();
        var validator2 = sp.GetRequiredService<IXmlSchemaValidator>();

        // Singleton: same instance
        validator1.Should().BeSameAs(validator2);
    }

    [Fact]
    public void AddXmlValidation_ValidatorSharesCacheWithExternalRegistration()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IXmlSchemaCache>();
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();

        // Register a schema via the cache and validate via the validator — same instance chain
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddXmlValidation_NullServices_ThrowsArgumentNullException()
    {
        var act = () => XmlValidationServiceCollectionExtensions.AddXmlValidation(null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("services");
    }

    [Fact]
    public void AddXmlValidation_WithConfigure_NullServices_ThrowsArgumentNullException()
    {
        var act = () => XmlValidationServiceCollectionExtensions.AddXmlValidation(null!, _ => { });

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("services");
    }

    [Fact]
    public void AddXmlValidation_NullServices_ThrowsBeforeInvokingConfigure()
    {
        bool configureInvoked = false;
        var act = () => XmlValidationServiceCollectionExtensions.AddXmlValidation(null!, _ => configureInvoked = true);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("services");
        configureInvoked.Should().BeFalse();
    }

    // ─── Options Overload ─────────────────────────────────────────────────────

    [Fact]
    public void AddXmlValidation_WithConfigure_RegistersValidator()
    {
        bool configureCalled = false;
        var services = new ServiceCollection();
        services.AddXmlValidation(options =>
        {
            configureCalled = true;
            options.IncludeWarnings = true;
            options.TreatWarningsAsErrors = true;
        });
        var sp = services.BuildServiceProvider();

        var validator = sp.GetService<IXmlSchemaValidator>();

        validator.Should().NotBeNull();
        configureCalled.Should().BeTrue();
    }

    [Fact]
    public void AddXmlValidation_WithNullConfigure_UsesDefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation(configure: null);
        var sp = services.BuildServiceProvider();

        // Should not throw and should return a usable validator
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();
        var cache = sp.GetRequiredService<IXmlSchemaCache>();

        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddXmlValidation_WithIncludeWarnings_ValidatorWorksCorrectly()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation(options => options.IncludeWarnings = true);
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IXmlSchemaCache>();
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();

        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var validResult = validator.Validate(ValidXml, TargetNamespace);
        validResult.IsSuccess.Should().BeTrue();

        var invalidResult = validator.Validate(InvalidXml, TargetNamespace);
        invalidResult.IsFailure.Should().BeTrue();
        invalidResult.Error.Code.Should().Be("XmlValidation.SchemaViolation");
    }

    // ─── Logger Integration ───────────────────────────────────────────────────

    [Fact]
    public void AddXmlValidation_WithLoggerRegistered_ValidatorReceivesLogger()
    {
        var services = new ServiceCollection();
        services.AddLogging(); // NullLogger — sufficient to verify ILogger injection
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        // Should resolve without exception
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();
        var cache = sp.GetRequiredService<IXmlSchemaCache>();

        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddXmlValidation_WithoutLoggerRegistered_ValidatorUsesNullLogger()
    {
        // No logging registered — validator should fall back to NullLogger
        var services = new ServiceCollection();
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var validator = sp.GetRequiredService<IXmlSchemaValidator>();
        var cache = sp.GetRequiredService<IXmlSchemaCache>();

        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    // ─── End-to-End Validation via DI ─────────────────────────────────────────

    [Fact]
    public void DI_EndToEnd_WithCustomOptionsAndLogger_ValidatesCorrectly()
    {
        var services = new ServiceCollection();
        var logger = new TestLogger<XmlSchemaValidator>();
        services.AddSingleton<ILogger<XmlSchemaValidator>>(logger);
        services.AddXmlValidation(options =>
        {
            options.IncludeWarnings = true;
            options.TreatWarningsAsErrors = false;
        });
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IXmlSchemaCache>();
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();

        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        logger.LoggedMessages.Should().NotBeEmpty();
        logger.LoggedMessages.Should().Contain(m => m.Contains(TargetNamespace));
    }

    [Fact]
    public void DI_EndToEnd_InvalidXml_ReturnsSchemaViolation()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IXmlSchemaCache>();
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();

        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var result = validator.Validate(InvalidXml, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void DI_EndToEnd_ValidXmlStream_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IXmlSchemaCache>();
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();

        cache.RegisterSchema(TargetNamespace, SampleXsd);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidXml));
        var result = validator.Validate(stream, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddXmlValidation_CalledMultipleTimes_IsIdempotent()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation();
        services.AddXmlValidation();

        services.Count(s => s.ServiceType == typeof(IXmlSchemaCache)).Should().Be(1);
        services.Count(s => s.ServiceType == typeof(IXmlSchemaValidator)).Should().Be(1);
    }

    [Fact]
    public void AddXmlValidation_PreExistingService_DoesNotOverwrite()
    {
        var services = new ServiceCollection();
        var customCache = new XmlSchemaCache();
        services.AddSingleton<IXmlSchemaCache>(customCache);

        services.AddXmlValidation();

        var sp = services.BuildServiceProvider();
        sp.GetRequiredService<IXmlSchemaCache>().Should().BeSameAs(customCache);
        services.Count(s => s.ServiceType == typeof(IXmlSchemaCache)).Should().Be(1);
    }

    [Fact]
    public void AddXmlValidation_WhenOptionsConfiguredViaIOptions_UsesConfiguredOptions()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<XmlValidationOptions>(opts =>
        {
            opts.MaxErrors = 42;
            opts.TreatWarningsAsErrors = true;
        });

        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IXmlSchemaCache>();
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var result = validator.Validate(WarningOnlyXml, WarningNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
    }

    [Fact]
    public void AddXmlValidation_WhenConfigureDelegateSupplied_OverridesIOptions()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<XmlValidationOptions>(opts =>
        {
            opts.TreatWarningsAsErrors = true;
        });

        services.AddXmlValidation(opts =>
        {
            opts.TreatWarningsAsErrors = false;
        });
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IXmlSchemaCache>();
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var result = validator.Validate(WarningOnlyXml, WarningNamespace);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddXmlValidation_WhenNoConfigureAndNoIOptions_UsesDefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddXmlValidation();
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IXmlSchemaCache>();
        var validator = sp.GetRequiredService<IXmlSchemaValidator>();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var result = validator.Validate(WarningOnlyXml, WarningNamespace);
        result.IsSuccess.Should().BeTrue();
    }
}
