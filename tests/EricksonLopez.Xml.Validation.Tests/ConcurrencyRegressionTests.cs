// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Xml.Validation.Tests;

public class ConcurrencyRegressionTests
{
    private const string TargetNamespace = "http://ericksonlopez.dev/xml/concurrency";
    private const string ValidXml = """
        <ConcurrencyRoot xmlns="http://ericksonlopez.dev/xml/concurrency">
            <Data>ThreadSafe</Data>
        </ConcurrencyRoot>
        """;

    private const string ValidXsd = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
                   targetNamespace="http://ericksonlopez.dev/xml/concurrency" 
                   elementFormDefault="qualified">
            <xs:element name="ConcurrencyRoot">
                <xs:complexType>
                    <xs:sequence>
                        <xs:element name="Data" type="xs:string"/>
                    </xs:sequence>
                </xs:complexType>
            </xs:element>
        </xs:schema>
        """;

    [Fact]
    public void Validator_UnderHighConcurrency_DoesNotProduceFalseNegatives()
    {
        // Arrange
        var cache = new XmlSchemaCache();
        var validator = new XmlSchemaValidator(cache);
        cache.RegisterSchema(TargetNamespace, ValidXsd);

        // Act & Assert
        // We will run 100 threads validating the XML concurrently,
        // while 10 threads continuously re-register the schema.
        // If the cache update is not atomic, a validating thread will read the cache 
        // after it has been updated, but before the global elements are populated, 
        // returning a false negative.

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

        Parallel.For(0, 10000, parallelOptions, i =>
        {
            if (i % 100 == 0)
            {
                // Re-register to trigger the potential race condition
                cache.RegisterSchema(TargetNamespace, ValidXsd);
            }
            else
            {
                // Validate
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidXml));
                var result = validator.Validate(stream, TargetNamespace);

                // If it fails with SchemaViolation or XmlMalformed, it's a bug!
                if (!result.IsSuccess)
                {
                    Assert.Fail($"Validation failed unexpectedly due to race condition. Error: {result.Error}");
                }
            }
        });
    }
}
