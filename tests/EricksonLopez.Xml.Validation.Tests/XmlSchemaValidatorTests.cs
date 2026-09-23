// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AwesomeAssertions;
using EricksonLopez.Xml.Validation.Tests.Common;
using Xunit;
using static EricksonLopez.Xml.Validation.Tests.Common.XmlTestSamples;

namespace EricksonLopez.Xml.Validation.Tests;

/// <summary>
/// Verifies synchronous XML schema validation paths, Anti-XXE enforcement,
/// and error mapping of <see cref="XmlSchemaValidator"/>.
/// </summary>
public sealed class XmlSchemaValidatorTests
{
    // ─── Happy Path ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidXml_ReturnsSuccess()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidXmlStream_ReturnsSuccess()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidXml));
        var result = validator.Validate(stream, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidXmlStream_DoesNotCloseStream()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidXml));
        validator.Validate(stream, TargetNamespace);

        // Stream must still be open and readable after validation (CloseInput = false)
        stream.CanRead.Should().BeTrue();
    }

    // ─── Schema Violations ────────────────────────────────────────────────────

    [Fact]
    public void Validate_MissingRequiredElement_ReturnsValidationError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate(InvalidXml, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().NotBeNullOrWhiteSpace();
        result.Error.Description.Should().Contain("Line ");
        result.Error.Description.Should().Contain("Pos ");
        result.Error.Description.Should().Contain("Total");
    }

    [Fact]
    public void Validate_SchemaViolation_ErrorMessageContainsLineInfo()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate(InvalidXml, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Line ");
        result.Error.Description.Should().Contain("Pos ");
    }

    // ─── Schema Not Registered ────────────────────────────────────────────────

    [Fact]
    public void Validate_UnregisteredSchema_ReturnsNotFoundError()
    {
        var cache = new XmlSchemaCache();
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate("<Invoice xmlns=\"http://unknown.dev\"><Id>1</Id></Invoice>", "http://unknown.dev");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaNotRegistered");
    }

    [Fact]
    public void Validate_UnregisteredSchema_ErrorMessageContainsNamespace()
    {
        var cache = new XmlSchemaCache();
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate("<x/>", "http://my.namespace");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("http://my.namespace");
    }

    // ─── Malformed XML ────────────────────────────────────────────────────────

    [Fact]
    public void Validate_MalformedXml_ReturnsMalformedError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate("<Invoice><Id>Unclosed tag", TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().StartWith("Malformed XML at Line ");
    }

    [Fact]
    public void Validate_MalformedXml_ErrorMessageContainsPosition()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate("<Invoice><Id>Unclosed tag", TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().StartWith("Malformed XML at Line ");
        result.Error.Description.Should().Contain("Position ");
    }

    [Fact]
    public void Validate_EmptyString_ReturnsMalformedError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate(string.Empty, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    [Fact]
    public void Validate_EmptyStream_ReturnsMalformedError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        using var emptyStream = new MemoryStream();
        var result = validator.Validate(emptyStream, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    [Fact]
    public void Validate_StreamAtNonZeroPosition_StillReadsFromCurrentPosition()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        // Put some garbage bytes before the valid XML — simulate a stream already partially consumed
        var preamble = Encoding.UTF8.GetBytes("GARBAGE");
        var xmlBytes = Encoding.UTF8.GetBytes(ValidXml);
        var allBytes = new byte[preamble.Length + xmlBytes.Length];
        preamble.CopyTo(allBytes, 0);
        xmlBytes.CopyTo(allBytes, preamble.Length);

        using var stream = new MemoryStream(allBytes);
        stream.Position = preamble.Length; // Skip the garbage — position at XML start

        var result = validator.Validate(stream, TargetNamespace);

        // The stream is at a valid position now → should succeed
        result.IsSuccess.Should().BeTrue();
    }

    // ─── Anti-XXE ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_DtdDeclared_ProhibitedAntiXxeFailsSafely()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var xxeXml = """
            <!DOCTYPE Invoice [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>&xxe;</Id>
                <Total>100</Total>
            </Invoice>
            """;

        var result = validator.Validate(xxeXml, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    // ─── UTF-8 Bytes / Span ───────────────────────────────────────────────────

    [Fact]
    public void Validate_Utf8Span_ValidXml_ReturnsSuccess()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var validXml = """
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>INV-002</Id>
                <Total>450.00</Total>
            </Invoice>
            """u8;

        var result = validator.Validate(validXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void Validate_Utf8Span_InvalidXml_ReturnsSchemaViolation()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var invalidXmlBytes = Encoding.UTF8.GetBytes(InvalidXml);

        var result = validator.Validate(invalidXmlBytes.AsSpan(), TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
    }

    // ─── Argument Validation ──────────────────────────────────────────────────

    [Fact]
    public void Validate_NullXml_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();
        var validator = new XmlSchemaValidator(cache);

        var act = () => validator.Validate((string)null!, TargetNamespace);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("xml");
    }

    [Fact]
    public void Validate_NullNamespace_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();
        var validator = new XmlSchemaValidator(cache);

        var act = () => validator.Validate(ValidXml, null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("targetNamespace");
    }

    [Fact]
    public void Validate_NullStream_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();
        var validator = new XmlSchemaValidator(cache);

        var act = () => validator.Validate((Stream)null!, TargetNamespace);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("xmlStream");
    }

    [Fact]
    public void Constructor_NullCache_ThrowsArgumentNullException()
    {
        var act = () => new XmlSchemaValidator(null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("schemaCache");
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();
        var act = () => new XmlSchemaValidator(cache, null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("options");
    }

    // ─── Warning Reporting ────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithWarningsDisabled_WarningsAreIgnored()
    {
        // XSD with a deprecated use="prohibited" pattern that may generate warnings
        var xsdWithPotentialWarnings = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/test"
                       xmlns="http://ericksonlopez.dev/test"
                       elementFormDefault="qualified">
              <xs:element name="Root">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="Name" type="xs:string" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """;
        const string ns = "http://ericksonlopez.dev/test";
        const string xml = """<Root xmlns="http://ericksonlopez.dev/test"><Name>Test</Name></Root>""";

        var cache = new XmlSchemaCache();
        cache.RegisterSchema(ns, xsdWithPotentialWarnings);

        // Default options (IncludeWarnings = false)
        var validator = new XmlSchemaValidator(cache);
        var result = validator.Validate(xml, ns);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithIncludeWarningsEnabled_ValidXml_StillSucceeds()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var options = new XmlValidationOptions { IncludeWarnings = true };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_TreatWarningsAsErrors_False_ValidXml_PassesRegardlessOfWarningConfig()
    {
        // Regression test for TreatWarningsAsErrors=false bug:
        // When IncludeWarnings=true and TreatWarningsAsErrors=false, valid XML must still succeed
        // even if warnings are present. Previously warnings were added to the 'errors' bucket.
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = false
        };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(ValidXml, TargetNamespace);

        // Must succeed — warnings (if any) must NOT cause failure when TreatWarningsAsErrors=false
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_TreatWarningsAsErrors_True_ValidXmlWithoutWarnings_StillSucceeds()
    {
        // TreatWarningsAsErrors=true only fails validation when warnings ARE present.
        // Clean XML with a clean schema should still pass.
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = true
        };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_TreatWarningsAsErrors_False_InvalidXml_StillFails()
    {
        // Even with TreatWarningsAsErrors=false, schema errors must still fail validation.
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = false
        };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(InvalidXml, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
    }

    [Fact]
    public void Validate_TreatWarningsAsErrors_True_InvalidXml_Fails()
    {
        // With TreatWarningsAsErrors=true, schema errors still cause failure (error trumps everything).
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = true
        };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(InvalidXml, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
    }

    [Fact]
    public void XmlValidationOptions_DefaultValues_AreCorrect()
    {
        // Contract test: defaults must be false for both options
        var options = new XmlValidationOptions();

        options.IncludeWarnings.Should().BeFalse();
        options.TreatWarningsAsErrors.Should().BeFalse();
    }

    [Fact]
    public void Validate_Stream_MalformedXml_ReturnsXmlMalformedError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<UnclosedElement"));
        var result = validator.Validate(stream, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().StartWith("Malformed XML at Line ");
    }

    [Fact]
    public void Validate_Span_MalformedXml_ReturnsXmlMalformedError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes("<UnclosedElement");
        var result = validator.Validate(span, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().StartWith("Malformed XML at Line ");
    }

    [Fact]
    public void Validate_WithLogger_ExecutesAndLogs()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var logger = new TestLogger<XmlSchemaValidator>();

        var validator = new XmlSchemaValidator(cache, new XmlValidationOptions(), logger);
        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
        logger.LoggedMessages.Should().NotBeEmpty();
        logger.LoggedMessages[0].Should().Contain(TargetNamespace);
    }

    [Fact]
    public void Validate_WhenWarningEmitted_AndTreatWarningsAsErrorsTrue_ReturnsFailure()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = true
        };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(WarningOnlyXml, WarningNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("[Warning]");
    }

    [Fact]
    public void Validate_WhenWarningEmitted_AndTreatWarningsAsErrorsFalse_ReturnsSuccess()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = false
        };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(WarningOnlyXml, WarningNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenBothErrorAndWarningEmitted_AndIncludeWarningsTrue_CombinesMessages()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = false
        };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(WarningAndErrorXml, WarningNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("Line");
        result.Error.Description.Should().Contain("[Warning]");
        result.Error.Description.Should().Contain(" | ");
    }

    [Fact]
    public void Validate_WhenBothErrorAndWarningEmitted_AndIncludeWarningsFalse_DoesNotIncludeWarningsInErrors()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var options = new XmlValidationOptions
        {
            IncludeWarnings = false,
            TreatWarningsAsErrors = false
        };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(WarningAndErrorXml, WarningNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("Line");
        result.Error.Description.Should().NotContain("[Warning]");
    }

    [Fact]
    public void Validate_NullArguments_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();
        var validator = new XmlSchemaValidator(cache);

        var act1 = () => validator.Validate((string)null!, TargetNamespace);
        act1.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("xml");

        var act2 = () => validator.Validate(ValidXml, null!);
        act2.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("targetNamespace");

        var act3 = () => validator.Validate((Stream)null!, TargetNamespace);
        act3.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("xmlStream");

        using var stream = new MemoryStream();
        var act4 = () => validator.Validate(stream, null!);
        act4.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("targetNamespace");

        try
        {
            validator.Validate(ReadOnlySpan<byte>.Empty, null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            ex.ParamName.Should().Be("targetNamespace");
        }
    }

    [Fact]
    public void Validate_WhenCacheReturnsTrueWithNullSchemaSet_ReturnsSchemaNotRegisteredError()
    {
        var mockCache = new NullSchemaCacheMock();
        var validator = new XmlSchemaValidator(mockCache);

        var result = validator.Validate(ValidXml, TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaNotRegistered");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidXml));
        var streamResult = validator.Validate(stream, TargetNamespace);
        streamResult.IsFailure.Should().BeTrue();
        streamResult.Error.Code.Should().Be("XmlValidation.SchemaNotRegistered");

        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(ValidXml);
        var spanResult = validator.Validate(span, TargetNamespace);
        spanResult.IsFailure.Should().BeTrue();
        spanResult.Error.Code.Should().Be("XmlValidation.SchemaNotRegistered");
    }

    [Fact]
    public void Validate_WithSchemaLocation_ValidationSucceedsSafely()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate(SchemaLocationXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMockCache_NullNamespace_ThrowsArgumentNullException()
    {
        var mockCache = new NullSchemaCacheMock();
        var validator = new XmlSchemaValidator(mockCache);

        var act1 = () => validator.Validate(ValidXml, null!);
        act1.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("targetNamespace");

        using var stream = new MemoryStream();
        var act2 = () => validator.Validate(stream, null!);
        act2.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("targetNamespace");
    }

    [Fact]
    public void Validate_XmlWithInlineSchema_WhenInvalid_FailsValidation()
    {
        var cache = new XmlSchemaCache();
        const string docXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/inline"
                       xmlns="http://ericksonlopez.dev/inline"
                       elementFormDefault="qualified">
              <xs:element name="Doc">
                <xs:complexType>
                  <xs:sequence>
                    <xs:any namespace="##any" processContents="lax" minOccurs="0" maxOccurs="unbounded" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """;
        cache.RegisterSchema("http://ericksonlopez.dev/inline", docXsd);
        var validator = new XmlSchemaValidator(cache, new XmlValidationOptions { ProcessInlineSchema = true });

        // Document with inline schema requiring Amount to be an integer, but value is "not_an_int"
        const string inlineXml = """
            <Doc xmlns="http://ericksonlopez.dev/inline"
                 xmlns:xs="http://www.w3.org/2001/XMLSchema">
              <xs:schema targetNamespace="http://ericksonlopez.dev/inline">
                <xs:element name="Amount" type="xs:integer" />
              </xs:schema>
              <Amount>not_an_int</Amount>
            </Doc>
            """;

        var result = validator.Validate(inlineXml, "http://ericksonlopez.dev/inline");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
    }

    [Fact]
    public void Validate_UnregisteredSchema_LogsWarning()
    {
        var cache = new XmlSchemaCache();
        var logger = new TestLogger<XmlSchemaValidator>();
        var validator = new XmlSchemaValidator(cache, new XmlValidationOptions(), logger);

        var result = validator.Validate(ValidXml, "http://unregistered");
        result.IsFailure.Should().BeTrue();
        logger.LoggedMessages.Should().Contain(m => m.Contains("Schema not registered"));
    }

    [Fact]
    public void Validate_MalformedXml_LogsError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var logger = new TestLogger<XmlSchemaValidator>();
        var validator = new XmlSchemaValidator(cache, new XmlValidationOptions(), logger);

        var result = validator.Validate("<unclosed", TargetNamespace);
        result.IsFailure.Should().BeTrue();
        logger.LoggedMessages.Should().Contain(m => m.Contains("Malformed XML"));
    }

    [Fact]
    public void Validate_InvalidXml_LogsValidationFailed()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var logger = new TestLogger<XmlSchemaValidator>();
        var validator = new XmlSchemaValidator(cache, new XmlValidationOptions(), logger);

        var result = validator.Validate(InvalidXml, TargetNamespace);
        result.IsFailure.Should().BeTrue();
        logger.LoggedMessages.Should().Contain(m => m.Contains("Validation failed"));
    }

    [Fact]
    public void Validate_Stream_UnregisteredSchema_LogsWarning()
    {
        var cache = new XmlSchemaCache();
        var logger = new TestLogger<XmlSchemaValidator>();
        var validator = new XmlSchemaValidator(cache, new XmlValidationOptions(), logger);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidXml));
        var result = validator.Validate(stream, "http://unregistered");
        result.IsFailure.Should().BeTrue();
        logger.LoggedMessages.Should().Contain(m => m.Contains("Schema not registered"));
    }

    [Fact]
    public void Validate_Stream_MalformedXml_LogsError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var logger = new TestLogger<XmlSchemaValidator>();
        var validator = new XmlSchemaValidator(cache, new XmlValidationOptions(), logger);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<unclosed"));
        var result = validator.Validate(stream, TargetNamespace);
        result.IsFailure.Should().BeTrue();
        logger.LoggedMessages.Should().Contain(m => m.Contains("Malformed XML"));
    }

    // ─── Regression Tests (Mega-Audit Remediation) ──────────────────────────

    [Fact]
    public void Validate_EmptySpan_ReturnsMalformedError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var logger = new TestLogger<XmlSchemaValidator>();
        var validator = new XmlSchemaValidator(cache, new XmlValidationOptions(), logger);

        var result = validator.Validate(ReadOnlySpan<byte>.Empty, TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        logger.LoggedMessages.Should().Contain(m => m.Contains("Malformed XML") && m.Contains("empty"));
    }

    [Fact]
    public void Validate_DefaultSpan_ReturnsMalformedError()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate(default(ReadOnlySpan<byte>), TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    [Fact]
    public void Validate_DuplicateUniqueKeys_FailsValidation()
    {
        const string uniqueNs = "http://ericksonlopez.dev/unique-test";
        const string uniqueXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/unique-test"
                       xmlns:tns="http://ericksonlopez.dev/unique-test"
                       xmlns="http://ericksonlopez.dev/unique-test"
                       elementFormDefault="qualified">
              <xs:element name="Orders">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="Order" maxOccurs="unbounded">
                      <xs:complexType>
                        <xs:sequence>
                          <xs:element name="Id" type="xs:string" />
                        </xs:sequence>
                      </xs:complexType>
                    </xs:element>
                  </xs:sequence>
                </xs:complexType>
                <xs:unique name="UniqueOrderId">
                  <xs:selector xpath="tns:Order" />
                  <xs:field xpath="tns:Id" />
                </xs:unique>
              </xs:element>
            </xs:schema>
            """;

        const string duplicateXml = """
            <Orders xmlns="http://ericksonlopez.dev/unique-test">
                <Order><Id>ORD-001</Id></Order>
                <Order><Id>ORD-001</Id></Order>
            </Orders>
            """;

        var cache = new XmlSchemaCache();
        cache.RegisterSchema(uniqueNs, uniqueXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate(duplicateXml, uniqueNs);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("UniqueOrderId");
    }

    [Fact]
    public void Validate_TreatWarningsAsErrorsWithoutExplicitIncludeWarnings_FailsValidation()
    {
        var options = new XmlValidationOptions
        {
            TreatWarningsAsErrors = true
        };

        options.IncludeWarnings.Should().BeTrue("TreatWarningsAsErrors should automatically enable IncludeWarnings");
    }

    [Fact]
    public void Validate_ErrorFlooding_LimitsCollectedErrorsToMaxErrors()
    {
        const string floodingNs = "http://ericksonlopez.dev/flooding";
        const string floodingXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/flooding"
                       xmlns="http://ericksonlopez.dev/flooding"
                       elementFormDefault="qualified">
              <xs:element name="Items">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="Item" maxOccurs="unbounded">
                      <xs:complexType>
                        <xs:sequence>
                          <xs:element name="Amount" type="xs:decimal" />
                        </xs:sequence>
                      </xs:complexType>
                    </xs:element>
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """;

        var cache = new XmlSchemaCache();
        cache.RegisterSchema(floodingNs, floodingXsd);
        var options = new XmlValidationOptions
        {
            MaxErrors = 5
        };
        var validator = new XmlSchemaValidator(cache, options);

        var sb = new StringBuilder();
        sb.Append("<Items xmlns=\"http://ericksonlopez.dev/flooding\">");
        for (int i = 0; i < 20; i++)
        {
            sb.Append("<Item><Amount>not-a-number</Amount></Item>");
        }
        sb.Append("</Items>");

        var result = validator.Validate(sb.ToString(), floodingNs);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        var errorCount = result.Error.Description.Split(" | ").Length;
        errorCount.Should().Be(5);
    }

    [Fact]
    public void Validate_WarningFlooding_LimitsCollectedWarningsToMaxErrors()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(WarningNamespace, WarningXsd);
        var options = new XmlValidationOptions
        {
            IncludeWarnings = true,
            TreatWarningsAsErrors = true,
            MaxErrors = 5
        };
        var validator = new XmlSchemaValidator(cache, options);

        var sb = new StringBuilder();
        sb.Append("<WarningRoot xmlns=\"http://ericksonlopez.dev/warning\"><Total>100.00</Total>");
        for (int i = 0; i < 20; i++)
        {
            sb.Append("<Undeclared").Append(i).Append(">val</Undeclared").Append(i).Append('>');
        }
        sb.Append("</WarningRoot>");

        var result = validator.Validate(sb.ToString(), WarningNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        var warningCount = result.Error.Description.Split(" | ").Length;
        warningCount.Should().Be(5);
    }

    [Fact]
    public void Validate_InlineSchemaDisabled_DoesNotProcessInlineSchema()
    {
        const string containerNs = "http://ericksonlopez.dev/container";
        const string containerXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/container"
                       xmlns="http://ericksonlopez.dev/container"
                       elementFormDefault="qualified">
              <xs:element name="Container">
                <xs:complexType>
                  <xs:sequence>
                    <xs:any namespace="##any" processContents="lax" minOccurs="0" maxOccurs="unbounded" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """;

        const string xmlWithBrokenInlineSchema = """
            <Container xmlns="http://ericksonlopez.dev/container" xmlns:xs="http://www.w3.org/2001/XMLSchema">
              <xs:schema targetNamespace="http://ericksonlopez.dev/inline">
                <xs:element name="Broken" type="invalid-undefined-type" />
              </xs:schema>
            </Container>
            """;

        var cache = new XmlSchemaCache();
        cache.RegisterSchema(containerNs, containerXsd);

        // When ProcessInlineSchema = true, the inline schema is compiled and fails
        var validatorEnabled = new XmlSchemaValidator(cache, new XmlValidationOptions { ProcessInlineSchema = true });
        var resultEnabled = validatorEnabled.Validate(xmlWithBrokenInlineSchema, containerNs);
        resultEnabled.IsFailure.Should().BeTrue();

        // When ProcessInlineSchema = false, the inline schema is ignored and treated as lax XML
        var validatorDisabled = new XmlSchemaValidator(cache, new XmlValidationOptions { ProcessInlineSchema = false });
        var resultDisabled = validatorDisabled.Validate(xmlWithBrokenInlineSchema, containerNs);
        resultDisabled.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Options_DefaultProcessInlineSchema_IsFalse()
    {
        var options = new XmlValidationOptions();
        options.ProcessInlineSchema.Should().BeFalse("ProcessInlineSchema must be false by default to prevent schema poisoning.");
    }

    [Fact]
    public void Validate_DefaultOptions_IgnoresInlineSchemaByDefault()
    {
        const string containerNs = "http://ericksonlopez.dev/container";
        const string containerXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/container"
                       xmlns="http://ericksonlopez.dev/container"
                       elementFormDefault="qualified">
              <xs:element name="Container">
                <xs:complexType>
                  <xs:sequence>
                    <xs:any namespace="##any" processContents="lax" minOccurs="0" maxOccurs="unbounded" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """;

        const string xmlWithBrokenInlineSchema = """
            <Container xmlns="http://ericksonlopez.dev/container" xmlns:xs="http://www.w3.org/2001/XMLSchema">
              <xs:schema targetNamespace="http://ericksonlopez.dev/inline">
                <xs:element name="Broken" type="invalid-undefined-type" />
              </xs:schema>
            </Container>
            """;

        var cache = new XmlSchemaCache();
        cache.RegisterSchema(containerNs, containerXsd);

        // Standard validator created with default options (no explicit options parameter)
        var validator = new XmlSchemaValidator(cache);
        var result = validator.Validate(xmlWithBrokenInlineSchema, containerNs);
        result.IsSuccess.Should().BeTrue("Default configuration must safely ignore inline schema definitions.");
    }

    [Fact]
    public void Options_IncludeWarningsAndTreatWarningsAsErrors_MaintainsConsistency()
    {
        var options = new XmlValidationOptions();
        options.IncludeWarnings.Should().BeFalse();
        options.TreatWarningsAsErrors.Should().BeFalse();

        // Setting IncludeWarnings independently
        options.IncludeWarnings = true;
        options.IncludeWarnings.Should().BeTrue();
        options.TreatWarningsAsErrors.Should().BeFalse();

        options.IncludeWarnings = false;
        options.IncludeWarnings.Should().BeFalse();

        // Setting TreatWarningsAsErrors enables IncludeWarnings
        options.TreatWarningsAsErrors = true;
        options.TreatWarningsAsErrors.Should().BeTrue();
        options.IncludeWarnings.Should().BeTrue();

        // Mutating IncludeWarnings = false while TreatWarningsAsErrors is true STILL returns true
        options.IncludeWarnings = false;
        options.IncludeWarnings.Should().BeTrue("IncludeWarnings must remain true while TreatWarningsAsErrors is true.");

        // Disabling TreatWarningsAsErrors allows IncludeWarnings to reflect backing field
        options.TreatWarningsAsErrors = false;
        options.IncludeWarnings.Should().BeFalse();
    }

    [Fact]
    public void Options_TreatWarningsAsErrors_Enabled_SetsBackingIncludeWarnings()
    {
        var options = new XmlValidationOptions();
        options.TreatWarningsAsErrors = true;
        options.TreatWarningsAsErrors.Should().BeTrue();
        options.IncludeWarnings.Should().BeTrue();

        options.TreatWarningsAsErrors = false;
        options.TreatWarningsAsErrors.Should().BeFalse();
        options.IncludeWarnings.Should().BeFalse();

        var options2 = new XmlValidationOptions();
        options2.TreatWarningsAsErrors = true;
        options2.TreatWarningsAsErrors = false;
        options2.IncludeWarnings.Should().BeFalse();
    }

    [Fact]
    public void Validate_TreatWarningsAsErrorsTrue_ThenIncludeWarningsFalse_StillFailsOnWarning()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(WarningNamespace, WarningXsd);

        var options = new XmlValidationOptions
        {
            TreatWarningsAsErrors = true,
            IncludeWarnings = false
        };
        var validator = new XmlSchemaValidator(cache, options);

        var result = validator.Validate(WarningOnlyXml, WarningNamespace);

        result.IsFailure.Should().BeTrue("Warnings must cause validation failure even if IncludeWarnings was set to false after TreatWarningsAsErrors.");
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("[Warning]");
    }

    [Fact]
    public void Validate_WhenSchemaSetXmlResolverMutatedExternally_NeutralizesResolverAndValidatesSafely()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        // Simulate external caller mutating the cached schema set's resolver before validation
        cache.TryGetSchemaSet(TargetNamespace, out var schemaSet);
        schemaSet!.XmlResolver = new XmlUrlResolver();

        var validator = new XmlSchemaValidator(cache);
        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
        schemaSet.GetXmlResolver().Should().BeNull("Validator must neutralize external resolver mutation before executing.");
    }

    [Fact]
    public void XmlValidationOptions_MaxErrors_ThrowsWhenLessThanOrEqualToZero()
    {
        var options = new XmlValidationOptions();
        var actZero = () => options.MaxErrors = 0;
        var actNeg = () => options.MaxErrors = -1;

        actZero.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value")
            .WithMessage("*MaxErrors must be greater than zero.*");

        actNeg.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value")
            .WithMessage("*MaxErrors must be greater than zero.*");
    }

    [Fact]
    public void XmlValidationOptions_MaxCharactersInDocument_ThrowsWhenNegative_AndAllowsZero()
    {
        var options = new XmlValidationOptions();
        var actNeg = () => options.MaxCharactersInDocument = -1;

        actNeg.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value")
            .WithMessage("*MaxCharactersInDocument must be greater than or equal to zero.*");

        options.MaxCharactersInDocument = 0;
        options.MaxCharactersInDocument.Should().Be(0);
    }

    [Fact]
    public void Validate_WhenValidXml_ReturnsTrue_AndNeverReportsSchemaViolation()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        var result = validator.Validate(ValidXml, TargetNamespace);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithCustomSchemaCacheNotImplementingInternalProvider_ThrowsInvalidOperationException()
    {
        var customCache = new ExternalSchemaCacheMock();
        var validator = new XmlSchemaValidator(customCache);

        var act = () => validator.Validate(ValidXml, TargetNamespace);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must implement the internal IXmlSchemaSetProvider interface to be used by the validator.*");
    }

    private sealed class ExternalSchemaCacheMock : IXmlSchemaCache
    {
        public int Count => 0;
        public bool ContainsSchema(string targetNamespace) => false;
        public bool IsRootElementDeclared(string targetNamespace, string localName, string namespaceUri) => false;
        public void RegisterSchema(string targetNamespace, string xsdContent) { }
        public void RegisterSchema(string targetNamespace, Stream xsdStream) { }
        public void RegisterSchema(string targetNamespace, ReadOnlySpan<byte> utf8Xsd) { }
        public void Clear() { }
    }
}

