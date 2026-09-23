// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates enterprise pipeline architecture incorporating defensive schema validation.
/// </summary>
public static class Level10EnterpriseArchitecture
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

    private const string TrustedXml = """
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ENTERPRISE-2026-X1</OrderId>
          <CustomerId>CORP-GLOBAL</CustomerId>
        </Order>
        """;

    private const string UntrustedTamperedXml = """
        <Order xmlns="https://ericksonlopez.dev/schemas/orders">
          <OrderId>ENTERPRISE-2026-X1</OrderId>
          <!-- Missing mandatory CustomerId -->
        </Order>
        """;

    /// <summary>
    /// Executes the enterprise architecture pipeline showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 10: ENTERPRISE ARCHITECTURE AND DEFENSIVE PERIMETER PIPELINE");
        Console.WriteLine("================================================================================\n");

        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, OrderXsd);
        var validator = new XmlSchemaValidator(cache);

        var pipeline = new EnterpriseXmlIngestionPipeline(validator);

        Console.WriteLine("[1] Processing trusted XML document through enterprise pipeline:");
        var successResponse = await pipeline.ExecuteAsync(TrustedXml, TargetNamespace);
        Console.WriteLine($"    • Pipeline Result: {successResponse.Status}");
        Console.WriteLine($"    • Message: {successResponse.Message}\n");

        Console.WriteLine("[2] Processing tampered XML document through enterprise pipeline:");
        var failedResponse = await pipeline.ExecuteAsync(UntrustedTamperedXml, TargetNamespace);
        Console.WriteLine($"    • Pipeline Result: {failedResponse.Status}");
        Console.WriteLine($"    • Message: {failedResponse.Message}");

        Console.WriteLine("\n✔ Level 10 completed successfully.\n");
    }

    /// <summary>
    /// Represents the outcome of an enterprise ingestion pipeline operation.
    /// Encapsulated privately inside Level10 to enforce One-Type-Per-File.
    /// </summary>
    private sealed record PipelineResponse(string Status, string Message);

    /// <summary>
    /// Coordinates defensive XML validation as a perimeter firewall within an ingestion pipeline.
    /// Encapsulated privately inside Level10 to enforce One-Type-Per-File.
    /// </summary>
    private sealed class EnterpriseXmlIngestionPipeline
    {
        private readonly IXmlSchemaValidator _validator;

        public EnterpriseXmlIngestionPipeline(IXmlSchemaValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<PipelineResponse> ExecuteAsync(string rawXml, string targetNamespace)
        {
            // Step 1 (Perimeter Firewall): Structural validation and Anti-XXE
            var validationResult = _validator.Validate(rawXml, targetNamespace);

            if (validationResult.IsFailure)
            {
                // Defensive short-circuit: No deserializer or application handler is executed
                return new PipelineResponse(
                    Status: "REJECTED_AT_PERIMETER",
                    Message: $"[400 Bad Request] XML rejected due to XSD contract violation: {validationResult.Error.Description}");
            }

            // Step 2: Safe deserialization and downstream processing guaranteed
            return await Task.FromResult(new PipelineResponse(
                Status: "ACCEPTED_AND_PROCESSED",
                Message: "[200 OK] XML document verified and processed successfully into the domain core."));
        }
    }
}
