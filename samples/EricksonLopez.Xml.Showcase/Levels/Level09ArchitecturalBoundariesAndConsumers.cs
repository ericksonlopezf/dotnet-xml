// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Showcase.Levels;

/// <summary>
/// Demonstrates message-driven pipeline integration and architectural boundary composition.
/// </summary>
public static class Level09ArchitecturalBoundariesAndConsumers
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
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    /// <summary>
    /// Executes the architectural boundaries showcase demonstration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  LEVEL 09: ARCHITECTURAL BOUNDARIES AND CONSUMER INTEGRATION");
        Console.WriteLine("================================================================================\n");

        Console.WriteLine("1. EXPLICIT ARCHITECTURAL BOUNDARY DECLARATION:");
        Console.WriteLine("   • EricksonLopez.Xml.Validation is a pure in-memory compute library (Tier 0).");
        Console.WriteLine("   • NO external broker dependencies (RabbitMQ, Kafka, Azure Service Bus)");
        Console.WriteLine("     or distributed lock engines (Redis, ZooKeeper) are included.");
        Console.WriteLine("   • Integration with such technologies is achieved through clean composition in upper layers");
        Console.WriteLine("     (e.g., EricksonLopez.Messaging or EricksonLopez.DistributedLock).\n");

        Console.WriteLine("2. MESSAGE PIPELINE INTEGRATION PATTERN:");
        Console.WriteLine("   Demonstrating a queue consumer receiving binary XML payloads");
        Console.WriteLine("   and validating the schema using IXmlSchemaValidator prior to dispatch:\n");

        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, OrderXsd);
        var validator = new XmlSchemaValidator(cache);

        // Simulate incoming message payload from a messaging queue
        byte[] incomingMessagePayload = Encoding.UTF8.GetBytes("""
            <Order xmlns="https://ericksonlopez.dev/schemas/orders">
              <OrderId>QUEUE-MSG-90210</OrderId>
            </Order>
            """);

        var queueConsumer = new MessageQueueXmlConsumer(validator);
        var processResult = await queueConsumer.ProcessIncomingMessageAsync(
            incomingMessagePayload, TargetNamespace, CancellationToken.None);

        Console.WriteLine($"    • Consumer result: IsSuccess={processResult.IsSuccess}");

        Console.WriteLine("\n✔ Level 09 completed successfully.\n");
    }

    /// <summary>
    /// Simulates a message queue consumer composing <see cref="IXmlSchemaValidator"/> as an ingress filter.
    /// Encapsulated privately inside Level09 to enforce One-Type-Per-File.
    /// </summary>
    private sealed class MessageQueueXmlConsumer
    {
        private readonly IXmlSchemaValidator _validator;

        public MessageQueueXmlConsumer(IXmlSchemaValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<Result<bool>> ProcessIncomingMessageAsync(
            byte[] payload,
            string targetNamespace,
            CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream(payload);

            // Step 1: Schema validation and Anti-XXE without deserialization
            var validationResult = await _validator.ValidateAsync(stream, targetNamespace, cancellationToken);
            if (validationResult.IsFailure)
            {
                Console.WriteLine($"    [QueueConsumer] Rejecting invalid or malicious message: {validationResult.Error.Description}");
                return validationResult;
            }

            Console.WriteLine("    [QueueConsumer] Message verified conforming to XSD schema. Accepted for domain processing.");
            return Result<bool>.Success(true);
        }
    }
}
