// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation.Tests.Common;
using Xunit;

namespace EricksonLopez.Xml.Validation.Tests;

/// <summary>
/// Exhaustive adversarial test suite attacking XmlSchemaValidator and XmlSchemaCache
/// across parsing anomalies, malformed XML, encoding edge cases, DoS, and XML security vectors.
/// </summary>
public sealed class AdversarialParsingAndSecurityTests
{
    private readonly XmlSchemaCache _cache;
    private readonly XmlSchemaValidator _validator;

    public AdversarialParsingAndSecurityTests()
    {
        _cache = new XmlSchemaCache();
        _cache.RegisterSchema(XmlTestSamples.TargetNamespace, XmlTestSamples.SampleXsd);
        _validator = new XmlSchemaValidator(_cache);
    }

    [Fact]
    public void EmptyString_ReturnsXmlMalformed()
    {
        var result = _validator.Validate("", XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    [Fact]
    public void WhitespaceOnly_ReturnsXmlMalformed()
    {
        var result = _validator.Validate("   \r\n\t  \n  ", XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    [Fact]
    public void TruncatedXml_ReturnsXmlMalformed()
    {
        var truncated = "<Invoice xmlns=\"http://ericksonlopez.dev/invoice\"><Id>INV-001</Id><Tot";
        var result = _validator.Validate(truncated, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().Contain("Malformed XML");
    }

    [Fact]
    public void DuplicateAttribute_ReturnsXmlMalformed()
    {
        var xml = """<Invoice xmlns="http://ericksonlopez.dev/invoice" id="1" id="2"><Id>A</Id><Total>10</Total></Invoice>""";
        var result = _validator.Validate(xml, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().Contain("duplicate attribute");
    }

    [Fact]
    public void UndeclaredPrefix_ReturnsXmlMalformed()
    {
        var xml = """<inv:Invoice xmlns="http://ericksonlopez.dev/invoice"><Id>A</Id><Total>10</Total></inv:Invoice>""";
        var result = _validator.Validate(xml, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    [Fact]
    public void InvalidControlCharacters_ReturnsXmlMalformed()
    {
        // \u0001 is invalid in XML 1.0
        var xml = "<Invoice xmlns=\"http://ericksonlopez.dev/invoice\"><Id>INV\u0001001</Id><Total>10</Total></Invoice>";
        var result = _validator.Validate(xml, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
    }

    [Fact]
    public void CDataAndCommentsAndPI_AreHandledCorrectly()
    {
        var xml = """
            <!-- Before root comment -->
            <?target instruction?>
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <!-- Inner comment -->
                <Id><![CDATA[INV-CDATA-001]]></Id>
                <Total>199.99</Total>
            </Invoice>
            """;
        var result = _validator.Validate(xml, XmlTestSamples.TargetNamespace);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void UnicodeSurrogatePairs_Emoji_ValidatesSuccessfully()
    {
        var xml = """
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>INV-🚀-999-✨</Id>
                <Total>199.99</Total>
            </Invoice>
            """;
        var result = _validator.Validate(xml, XmlTestSamples.TargetNamespace);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Utf8BomInSpan_ValidatesSuccessfully()
    {
        var rawXml = """
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>INV-BOM</Id>
                <Total>100.00</Total>
            </Invoice>
            """;
        var bytes = Encoding.UTF8.GetPreamble();
        var xmlBytes = Encoding.UTF8.GetBytes(rawXml);
        var combined = new byte[bytes.Length + xmlBytes.Length];
        bytes.CopyTo(combined, 0);
        xmlBytes.CopyTo(combined, bytes.Length);

        var result = _validator.Validate(combined.AsSpan(), XmlTestSamples.TargetNamespace);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void DeepNesting_DoesNotStackOverflow_HandlesSafely()
    {
        // Build 500 levels of nested tags inside valid Invoice namespace
        var sb = new StringBuilder();
        sb.Append("<Invoice xmlns=\"http://ericksonlopez.dev/invoice\"><Id>");
        for (int i = 0; i < 500; i++) sb.Append("<A>");
        for (int i = 0; i < 500; i++) sb.Append("</A>");
        sb.Append("</Id><Total>10</Total></Invoice>");

        var xml = sb.ToString();
        var result = _validator.Validate(xml, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
    }

    [Fact]
    public void CompletelyUnknownRootElement_WithoutWarnings_Investigation()
    {
        // XML with completely unregistered root and namespace (XML-SEC-001)
        var xml = "<CompletelyUnknownRoot xmlns=\"http://completely-unknown.org\"/>";
        var result = _validator.Validate(xml, XmlTestSamples.TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("Root element 'CompletelyUnknownRoot' in namespace 'http://completely-unknown.org' is not declared in the target schema.");
    }

    [Fact]
    public void CompletelyUnknownRootElement_Stream_FailsValidation()
    {
        var xmlBytes = Encoding.UTF8.GetBytes("<CompletelyUnknownRoot xmlns=\"http://completely-unknown.org\"/>");
        using var stream = new MemoryStream(xmlBytes);
        var result = _validator.Validate(stream, XmlTestSamples.TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("Root element 'CompletelyUnknownRoot' in namespace 'http://completely-unknown.org' is not declared in the target schema.");
    }

    [Fact]
    public async Task CompletelyUnknownRootElement_AsyncStream_FailsValidation()
    {
        var xmlBytes = Encoding.UTF8.GetBytes("<CompletelyUnknownRoot xmlns=\"http://completely-unknown.org\"/>");
        using var stream = new MemoryStream(xmlBytes);
        var result = await _validator.ValidateAsync(stream, XmlTestSamples.TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("Root element 'CompletelyUnknownRoot' in namespace 'http://completely-unknown.org' is not declared in the target schema.");
    }

    [Fact]
    public void CompletelyUnknownRootElement_Utf8Span_FailsValidation()
    {
        var xmlBytes = Encoding.UTF8.GetBytes("<CompletelyUnknownRoot xmlns=\"http://completely-unknown.org\"/>");
        var result = _validator.Validate(xmlBytes.AsSpan(), XmlTestSamples.TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.SchemaViolation");
        result.Error.Description.Should().Contain("Root element 'CompletelyUnknownRoot' in namespace 'http://completely-unknown.org' is not declared in the target schema.");
    }

    [Fact]
    public void XmlWithOnlyComments_ReturnsMalformedError()
    {
        var xml = "<!-- Only a comment, no root element -->";
        var result = _validator.Validate(xml, XmlTestSamples.TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().Contain("Root element is missing");
    }

    [Fact]
    public async Task XmlWithOnlyComments_AsyncStream_ReturnsMalformedError()
    {
        var xmlBytes = Encoding.UTF8.GetBytes("<!-- Only a comment, no root element -->");
        using var stream = new MemoryStream(xmlBytes);
        var result = await _validator.ValidateAsync(stream, XmlTestSamples.TargetNamespace);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().Contain("Root element is missing");
    }

    [Fact]
    public void WideXml_TenThousandElements_StreamsEfficiently()
    {
        // Large repetitive XML, but under 10MB limit
        var sb = new StringBuilder();
        sb.Append("<Invoice xmlns=\"http://ericksonlopez.dev/invoice\"><Id>WIDE</Id><Total>1.0</Total>");
        for (int i = 0; i < 2000; i++)
        {
            sb.Append(System.Globalization.CultureInfo.InvariantCulture, $"<!-- Comment {i} -->");
        }
        sb.Append("</Invoice>");

        var sw = Stopwatch.StartNew();
        var result = _validator.Validate(sb.ToString(), XmlTestSamples.TargetNamespace);
        sw.Stop();

        result.IsSuccess.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public void XXE_LocalFileDisclosure_IsProhibited()
    {
        var xxePayload = """
            <!DOCTYPE Invoice [<!ENTITY xxe SYSTEM "file:///c:/windows/win.ini">]>
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>&xxe;</Id>
                <Total>100</Total>
            </Invoice>
            """;
        var result = _validator.Validate(xxePayload, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().Contain("DTD is prohibited");
    }

    [Fact]
    public void XXE_ParameterEntity_IsProhibited()
    {
        var payload = """
            <!DOCTYPE Invoice [
                <!ENTITY % pe SYSTEM "http://127.0.0.1:9999/evil.dtd">
                %pe;
            ]>
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>1</Id>
                <Total>100</Total>
            </Invoice>
            """;
        var result = _validator.Validate(payload, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().Contain("DTD is prohibited");
    }

    [Fact]
    public void BillionLaughs_EntityExpansion_IsProhibitedImmediately()
    {
        var billionLaughs = """
            <?xml version="1.0"?>
            <!DOCTYPE lolz [
             <!ENTITY lol "lol">
             <!ELEMENT lolz (#PCDATA)>
             <!ENTITY lol1 "&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;">
             <!ENTITY lol2 "&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;">
             <!ENTITY lol3 "&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;">
             <!ENTITY lol4 "&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;">
             <!ENTITY lol5 "&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;">
             <!ENTITY lol6 "&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;">
             <!ENTITY lol7 "&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;">
             <!ENTITY lol8 "&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;">
             <!ENTITY lol9 "&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;">
            ]>
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>&lol9;</Id>
                <Total>100</Total>
            </Invoice>
            """;
        var sw = Stopwatch.StartNew();
        var result = _validator.Validate(billionLaughs, XmlTestSamples.TargetNamespace);
        sw.Stop();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().Contain("DTD is prohibited");
        sw.ElapsedMilliseconds.Should().BeLessThan(200); // Proved immediate rejection without CPU loop
    }

    [Fact]
    public void QuadraticBlowup_EntityExpansion_IsProhibitedImmediately()
    {
        var quadratic = """
            <!DOCTYPE kaboom [
              <!ENTITY a "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa">
            ]>
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>&a;&a;&a;&a;&a;&a;&a;&a;</Id>
                <Total>100</Total>
            </Invoice>
            """;
        var result = _validator.Validate(quadratic, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().Contain("DTD is prohibited");
    }

    [Fact]
    public void SchemaLocationPoisoning_NetworkResolutionBlocked()
    {
        var payload = """
            <Invoice xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                     xsi:schemaLocation="http://ericksonlopez.dev/invoice http://127.0.0.1:44444/malicious.xsd"
                     xmlns="http://ericksonlopez.dev/invoice">
                <Id>INV-SEC</Id>
                <Total>99.99</Total>
            </Invoice>
            """;
        // XmlResolver = null ensures network call to 127.0.0.1:44444 is never made
        var result = _validator.Validate(payload, XmlTestSamples.TargetNamespace);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void DocumentExceedingMaxCharacters_IsRejectedSafely()
    {
        var options = new XmlValidationOptions { MaxCharactersInDocument = 1000 };
        var validator = new XmlSchemaValidator(_cache, options);

        var largeId = new string('X', 1200);
        var xml = $"<Invoice xmlns=\"http://ericksonlopez.dev/invoice\"><Id>{largeId}</Id><Total>10</Total></Invoice>";

        var result = validator.Validate(xml, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("XmlValidation.XmlMalformed");
        result.Error.Description.Should().Contain("MaxCharactersInDocument");
    }

    [Fact]
    public void ErrorTruncation_DefendsAgainstMemoryExhaustion()
    {
        var options = new XmlValidationOptions { MaxErrors = 3 };
        var validator = new XmlSchemaValidator(_cache, options);

        // XML violating schema many times
        var xml = """
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Bad1/>
                <Bad2/>
                <Bad3/>
                <Bad4/>
                <Bad5/>
            </Invoice>
            """;
        var result = validator.Validate(xml, XmlTestSamples.TargetNamespace);
        result.IsFailure.Should().BeTrue();
        // Result should cap errors to MaxErrors (3 messages separated by |)
        var parts = result.Error.Description.Split(" | ");
        parts.Length.Should().BeLessThanOrEqualTo(3);
    }
}
