// Copyright © Erickson Lopez. MIT License.
// Native AOT smoke-test for EricksonLopez.Xml.Validation.
// This program is compiled and published with PublishAot=true to verify
// that the library works correctly under NativeAOT without reflection or
// trimming issues.
//
// Run: dotnet publish -r win-x64 -c Release
//      dotnet publish -r linux-x64 -c Release

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Xml.Validation;

Console.WriteLine("EricksonLopez.Xml.Validation — Native AOT Smoke Test");
Console.WriteLine("=".PadRight(55, '='));

const string targetNamespace = "http://ericksonlopez.dev/invoice";
const string sampleXsd = """
    <?xml version="1.0" encoding="utf-8"?>
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
               targetNamespace="http://ericksonlopez.dev/invoice"
               xmlns="http://ericksonlopez.dev/invoice"
               elementFormDefault="qualified">
      <xs:element name="Invoice">
        <xs:complexType>
          <xs:sequence>
            <xs:element name="Id" type="xs:string" />
            <xs:element name="Total" type="xs:decimal" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:schema>
    """;

const string validXml = """
    <Invoice xmlns="http://ericksonlopez.dev/invoice">
        <Id>INV-AOT-001</Id>
        <Total>999.99</Total>
    </Invoice>
    """;

const string invalidXml = """
    <Invoice xmlns="http://ericksonlopez.dev/invoice">
        <Id>INV-AOT-002</Id>
    </Invoice>
    """;

const string xxeXml = """
    <!DOCTYPE Invoice [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
    <Invoice xmlns="http://ericksonlopez.dev/invoice">
        <Id>&xxe;</Id>
        <Total>100</Total>
    </Invoice>
    """;

int passed = 0;
int failed = 0;

void Assert(string testName, bool condition, string? details = null)
{
    if (condition)
    {
        Console.WriteLine($"  [PASS] {testName}");
        passed++;
    }
    else
    {
        Console.WriteLine($"  [FAIL] {testName}{(details is not null ? $": {details}" : "")}");
        failed++;
    }
}

// ── Test 1: Cache Registration ─────────────────────────────────────────────
Console.WriteLine("\n[1] Cache Registration");
var cache = new XmlSchemaCache();
cache.RegisterSchema(targetNamespace, sampleXsd);
Assert("RegisterSchema (string)", cache.ContainsSchema(targetNamespace));
Assert("Count == 1", cache.Count == 1);
Assert("IsRootElementDeclared (Invoice)", cache.IsRootElementDeclared(targetNamespace, "Invoice", targetNamespace));
Assert("!IsRootElementDeclared (NonExistent)", !cache.IsRootElementDeclared(targetNamespace, "NonExistent", targetNamespace));

// ── Test 2: Registration from Span ────────────────────────────────────────
Console.WriteLine("\n[2] Registration from ReadOnlySpan<byte>");
cache.Clear();
var xsdBytes = Encoding.UTF8.GetBytes(sampleXsd);
cache.RegisterSchema(targetNamespace, xsdBytes.AsSpan());
Assert("RegisterSchema (Span<byte>)", cache.ContainsSchema(targetNamespace));

// ── Test 3: Registration from Stream ──────────────────────────────────────
Console.WriteLine("\n[3] Registration from Stream");
cache.Clear();
using var xsdStream = new MemoryStream(Encoding.UTF8.GetBytes(sampleXsd));
cache.RegisterSchema(targetNamespace, xsdStream);
Assert("RegisterSchema (Stream)", cache.ContainsSchema(targetNamespace));

// ── Test 4: Validate (string) — Happy Path ─────────────────────────────────
Console.WriteLine("\n[4] Validate (string) — Happy Path");
var validator = new XmlSchemaValidator(cache);
var result1 = validator.Validate(validXml, targetNamespace);
Assert("Valid XML → IsSuccess", result1.IsSuccess);
Assert("Valid XML → Value is true", result1.IsSuccess && result1.Value);

// ── Test 5: Validate (string) — Schema Violation ──────────────────────────
Console.WriteLine("\n[5] Validate (string) — Schema Violation");
var result2 = validator.Validate(invalidXml, targetNamespace);
Assert("Invalid XML → IsFailure", result2.IsFailure);
Assert("Invalid XML → Code == SchemaViolation", result2.IsFailure && result2.Error.Code == "XmlValidation.SchemaViolation");

// ── Test 6: Validate (string) — Anti-XXE ──────────────────────────────────
Console.WriteLine("\n[6] Validate (string) — Anti-XXE Protection");
var result3 = validator.Validate(xxeXml, targetNamespace);
Assert("XXE XML → IsFailure", result3.IsFailure);
Assert("XXE XML → Code == XmlMalformed", result3.IsFailure && result3.Error.Code == "XmlValidation.XmlMalformed");

// ── Test 7: Validate (Stream) ──────────────────────────────────────────────
Console.WriteLine("\n[7] Validate (Stream)");
using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(validXml));
var result4 = validator.Validate(xmlStream, targetNamespace);
Assert("Valid XML Stream → IsSuccess", result4.IsSuccess);

// ── Test 8: Validate (ReadOnlySpan<byte>) ─────────────────────────────────
Console.WriteLine("\n[8] Validate (ReadOnlySpan<byte>)");
var xmlBytes = Encoding.UTF8.GetBytes(validXml);
var result5 = validator.Validate(xmlBytes.AsSpan(), targetNamespace);
Assert("Valid XML Bytes → IsSuccess", result5.IsSuccess);

// ── Test 9: ValidateAsync (Stream) ────────────────────────────────────────
Console.WriteLine("\n[9] ValidateAsync (Stream)");
using var asyncStream = new MemoryStream(Encoding.UTF8.GetBytes(validXml));
var result6 = await validator.ValidateAsync(asyncStream, targetNamespace, CancellationToken.None);
Assert("ValidateAsync Valid XML → IsSuccess", result6.IsSuccess);

// ── Test 10: ValidateAsync — Unregistered Schema ──────────────────────────
Console.WriteLine("\n[10] ValidateAsync — Unregistered Schema");
using var unregStream = new MemoryStream(Encoding.UTF8.GetBytes("<x/>"));
var result7 = await validator.ValidateAsync(unregStream, "urn:not.registered");
Assert("Unregistered → IsFailure", result7.IsFailure);
Assert("Unregistered → Code == SchemaNotRegistered", result7.IsFailure && result7.Error.Code == "XmlValidation.SchemaNotRegistered");

// ── Test 11: XmlValidationOptions ─────────────────────────────────────────
Console.WriteLine("\n[11] XmlValidationOptions — IncludeWarnings");
var options = new XmlValidationOptions { IncludeWarnings = true };
var validatorWithOptions = new XmlSchemaValidator(cache, options);
var result8 = validatorWithOptions.Validate(validXml, targetNamespace);
Assert("IncludeWarnings=true, valid XML → IsSuccess", result8.IsSuccess);

// ── Test 12: Pre-cancelled Token ──────────────────────────────────────────
Console.WriteLine("\n[12] ValidateAsync — Pre-cancelled Token");
using var cts = new CancellationTokenSource();
cts.Cancel();
using var cancelStream = new MemoryStream(Encoding.UTF8.GetBytes(validXml));
try
{
    await validator.ValidateAsync(cancelStream, targetNamespace, cts.Token);
    Assert("Pre-cancelled token → should have thrown", false, "Expected OperationCanceledException");
}
catch (OperationCanceledException)
{
    Assert("Pre-cancelled token → OperationCanceledException", true);
}

// ── Test 13: Cache — Clear and Count ──────────────────────────────────────
Console.WriteLine("\n[13] Cache — Clear and Count");
var cache2 = new XmlSchemaCache();
cache2.RegisterSchema(targetNamespace, sampleXsd);
Assert("Count == 1 before clear", cache2.Count == 1);
cache2.Clear();
Assert("Count == 0 after clear", cache2.Count == 0);
Assert("ContainsSchema false after clear", !cache2.ContainsSchema(targetNamespace));
Assert("!IsRootElementDeclared after clear", !cache2.IsRootElementDeclared(targetNamespace, "Invoice", targetNamespace));

// ── Summary ───────────────────────────────────────────────────────────────
Console.WriteLine("\n" + "=".PadRight(55, '='));
Console.WriteLine($"Results: {passed} passed, {failed} failed");

if (failed > 0)
{
    Console.Error.WriteLine($"NATIVE AOT SMOKE TEST FAILED: {failed} assertion(s) failed.");
    return 1;
}

Console.WriteLine("NATIVE AOT SMOKE TEST PASSED. All assertions verified.");
return 0;
