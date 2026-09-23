// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Xml.Validation.Tests.Common;
using Xunit;
using static EricksonLopez.Xml.Validation.Tests.Common.XmlTestSamples;

namespace EricksonLopez.Xml.Validation.Tests;

/// <summary>
/// Verifies asynchronous schema validation paths of <see cref="XmlSchemaValidator"/>,
/// including cancellation handling and edge cases.
/// </summary>
public sealed class XmlSchemaValidatorAsyncTests
{
    private static XmlSchemaCache BuildCacheWithSchema()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        return cache;
    }

    private static MemoryStream ToStream(string xml)
        => new(Encoding.UTF8.GetBytes(xml));

    // ─── Happy Path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_ValidXml_ReturnsSuccess()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var stream = ToStream(ValidXml);
        var result = await validator.ValidateAsync(stream, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ValidXml_WithDefaultCancellationToken_Succeeds()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var stream = ToStream(ValidXml);
        // CancellationToken.None is the default
        var result = await validator.ValidateAsync(stream, TargetNamespace, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ValidXml_StreamDoesNotGetClosed()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var stream = ToStream(ValidXml);
        await validator.ValidateAsync(stream, TargetNamespace);

        // Stream should remain open after validation (CloseInput = false)
        stream.CanRead.Should().BeTrue();
    }

    // ─── Schema Violations ────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_MissingRequiredElement_ReturnsSchemaViolation()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var stream = ToStream(InvalidXml);
        var result = await validator.ValidateAsync(stream, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().NotBeNullOrWhiteSpace();
        result.Error.Description.Should().Contain("Line ");
        result.Error.Description.Should().Contain("Pos ");
        result.Error.Description.Should().Contain("Total");
    }

    [Fact]
    public async Task ValidateAsync_SchemaViolation_ErrorMessageContainsLineInfo()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var stream = ToStream(InvalidXml);
        var result = await validator.ValidateAsync(stream, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Line ");
        result.Error.Description.Should().Contain("Pos ");
    }

    // ─── Schema Not Registered ────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_UnregisteredSchema_ReturnsNotFoundError()
    {
        var validator = new XmlSchemaValidator(new XmlSchemaCache());

        using var stream = ToStream("<x/>");
        var result = await validator.ValidateAsync(stream, "http://unknown.dev");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaNotRegistered");
    }

    [Fact]
    public async Task ValidateAsync_UnregisteredSchema_ErrorContainsNamespace()
    {
        var validator = new XmlSchemaValidator(new XmlSchemaCache());

        using var stream = ToStream("<x/>");
        var result = await validator.ValidateAsync(stream, "http://my.special.ns");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("http://my.special.ns");
    }

    // ─── Malformed XML ────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_MalformedXml_ReturnsMalformedError()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var stream = ToStream("<Invoice><Id>Unclosed tag");
        var result = await validator.ValidateAsync(stream, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().StartWith("Malformed XML at Line ");
    }

    [Fact]
    public async Task ValidateAsync_MalformedXml_ErrorContainsPositionInfo()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var stream = ToStream("<Invoice><Id>Unclosed tag");
        var result = await validator.ValidateAsync(stream, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().StartWith("Malformed XML at Line ");
        result.Error.Description.Should().Contain("Position ");
    }

    [Fact]
    public async Task ValidateAsync_EmptyStream_ReturnsFailure()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var emptyStream = new MemoryStream();
        var result = await validator.ValidateAsync(emptyStream, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    // ─── Anti-XXE ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_DtdXml_ProhibitedAntiXxeFailsSafely()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var stream = ToStream(XxeXml);
        var result = await validator.ValidateAsync(stream, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    // ─── Cancellation ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_AlreadyCancelledToken_ThrowsOperationCancelledBeforeReading()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancelled

        using var stream = ToStream(ValidXml);

        // Must throw OperationCanceledException immediately (checked before reading)
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => validator.ValidateAsync(stream, TargetNamespace, cts.Token));
    }

    [Fact]
    public async Task ValidateAsync_AlreadyCancelledToken_SchemaNotFound_StillThrowsCancelled()
    {
        // Even when schema is not registered, a pre-cancelled token should throw
        var validator = new XmlSchemaValidator(new XmlSchemaCache());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        using var stream = ToStream("<x/>");

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => validator.ValidateAsync(stream, "http://any.ns", cts.Token));
    }

    [Fact]
    public async Task ValidateAsync_CancelledDuringReadLoop_ThrowsOperationCanceledException()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());
        using var cts = new CancellationTokenSource();
        using var stream = new SlowAsyncStream(Encoding.UTF8.GetBytes(ValidXml), cts);

        var act = async () => await validator.ValidateAsync(stream, TargetNamespace, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ─── Argument Validation ──────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_NullStream_ThrowsArgumentNullException()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        var act = () => validator.ValidateAsync(null!, TargetNamespace);

        var ex = await act.Should().ThrowAsync<ArgumentNullException>();
        ex.Which.ParamName.Should().Be("xmlStream");
    }

    [Fact]
    public async Task ValidateAsync_NullNamespace_ThrowsArgumentNullException()
    {
        var validator = new XmlSchemaValidator(BuildCacheWithSchema());

        using var stream = ToStream(ValidXml);
        var act = () => validator.ValidateAsync(stream, null!);

        var ex = await act.Should().ThrowAsync<ArgumentNullException>();
        ex.Which.ParamName.Should().Be("targetNamespace");
    }

    [Fact]
    public async Task ValidateAsync_WithMockCache_NullNamespace_ThrowsArgumentNullException()
    {
        var mockCache = new NullSchemaCacheMock();
        var validator = new XmlSchemaValidator(mockCache);

        using var stream = ToStream(ValidXml);
        var act = () => validator.ValidateAsync(stream, null!);

        var ex = await act.Should().ThrowAsync<ArgumentNullException>();
        ex.Which.ParamName.Should().Be("targetNamespace");
    }

    // ─── Options Integration ──────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_WithIncludeWarnings_ValidXml_StillSucceeds()
    {
        var options = new XmlValidationOptions { IncludeWarnings = true };
        var validator = new XmlSchemaValidator(BuildCacheWithSchema(), options);

        using var stream = ToStream(ValidXml);
        var result = await validator.ValidateAsync(stream, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithIncludeWarnings_InvalidXml_ReturnsFailure()
    {
        var options = new XmlValidationOptions { IncludeWarnings = true };
        var validator = new XmlSchemaValidator(BuildCacheWithSchema(), options);

        using var stream = ToStream(InvalidXml);
        var result = await validator.ValidateAsync(stream, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ValidateAsync_WhenWarningEmitted_AndTreatWarningsAsErrorsTrue_ReturnsFailure()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = true
        };
        var validator = new XmlSchemaValidator(cache, options);

        using var stream = ToStream(WarningOnlyXml);
        var result = await validator.ValidateAsync(stream, WarningNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("[Warning]");
    }

    [Fact]
    public async Task ValidateAsync_WhenWarningEmitted_AndTreatWarningsAsErrorsFalse_ReturnsSuccess()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = false
        };
        var validator = new XmlSchemaValidator(cache, options);

        using var stream = ToStream(WarningOnlyXml);
        var result = await validator.ValidateAsync(stream, WarningNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenCacheReturnsTrueWithNullSchemaSet_ReturnsSchemaNotRegisteredError()
    {
        var mockCache = new NullSchemaCacheMock();
        var validator = new XmlSchemaValidator(mockCache);

        using var stream = ToStream(ValidXml);
        var result = await validator.ValidateAsync(stream, TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaNotRegistered");
    }

    [Fact]
    public async Task ValidateAsync_UnregisteredSchema_LogsWarning()
    {
        var cache = new XmlSchemaCache();
        var logger = new TestLogger<XmlSchemaValidator>();
        var validator = new XmlSchemaValidator(cache, new XmlValidationOptions(), logger);

        using var stream = ToStream(ValidXml);
        var result = await validator.ValidateAsync(stream, "http://unregistered");
        result.IsFailure.Should().BeTrue();
        logger.LoggedMessages.Should().Contain(m => m.Contains("Schema not registered"));
    }

    [Fact]
    public async Task ValidateAsync_MalformedXml_LogsError()
    {
        var logger = new TestLogger<XmlSchemaValidator>();
        var validator = new XmlSchemaValidator(BuildCacheWithSchema(), new XmlValidationOptions(), logger);

        using var stream = ToStream("<unclosed");
        var result = await validator.ValidateAsync(stream, TargetNamespace);
        result.IsFailure.Should().BeTrue();
        logger.LoggedMessages.Should().Contain(m => m.Contains("Malformed XML"));
    }

    [Fact]
    public async Task ValidateAsync_CancelledDuringReadLoop_LogsCancellation()
    {
        var logger = new TestLogger<XmlSchemaValidator>();
        var validator = new XmlSchemaValidator(BuildCacheWithSchema(), new XmlValidationOptions(), logger);
        using var cts = new CancellationTokenSource();
        using var stream = new SlowAsyncStream(Encoding.UTF8.GetBytes(ValidXml), cts);

        var act = async () => await validator.ValidateAsync(stream, TargetNamespace, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
        logger.LoggedMessages.Should().Contain(m => m.Contains("Validation cancelled"));
    }
}
