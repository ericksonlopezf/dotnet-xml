// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Validation.Benchmarks;

/// <summary>
/// Defines performance benchmarks for XML schema validation and cache lookup operations.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class XmlValidationBenchmarks : IDisposable
{
    private const string TargetNamespace = "http://ericksonlopez.dev/invoice";

    private const string SampleXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="http://ericksonlopez.dev/invoice"
                   xmlns="http://ericksonlopez.dev/invoice"
                   elementFormDefault="qualified">
          <xs:element name="Invoice">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Id" type="xs:string" />
                <xs:element name="Amount" type="xs:decimal" />
                <xs:element name="Customer" type="xs:string" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    private const string ValidXmlString = """
        <Invoice xmlns="http://ericksonlopez.dev/invoice">
            <Id>INV-2026-001</Id>
            <Amount>1250.75</Amount>
            <Customer>Enterprise Corp</Customer>
        </Invoice>
        """;

    private XmlSchemaCache _cache = null!;
    private XmlSchemaValidator _validator = null!;
    private byte[] _validXmlBytes = null!;
    private MemoryStream _validXmlStream = null!;

    /// <summary>
    /// Initializes benchmark state, schema cache, and input buffers.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _cache = new XmlSchemaCache();
        _cache.RegisterSchema(TargetNamespace, SampleXsd);
        _validator = new XmlSchemaValidator(_cache);
        _validXmlBytes = Encoding.UTF8.GetBytes(ValidXmlString);
        _validXmlStream = new MemoryStream(_validXmlBytes);
    }

    /// <summary>
    /// Releases the stream resources used by the benchmark instance.
    /// </summary>
    [GlobalCleanup]
    public void Dispose()
    {
        _validXmlStream?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Measures performance of validating an XML document from a string.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing the validation result.</returns>
    [Benchmark(Baseline = true)]
    public Result<bool> ValidateFromString()
    {
        return _validator.Validate(ValidXmlString, TargetNamespace);
    }

    /// <summary>
    /// Measures performance of validating an XML document from a readable stream.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing the validation result.</returns>
    [Benchmark]
    public Result<bool> ValidateFromStream()
    {
        _validXmlStream.Position = 0;
        return _validator.Validate(_validXmlStream, TargetNamespace);
    }

    /// <summary>
    /// Measures performance of validating an XML document from a read-only byte span.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing the validation result.</returns>
    [Benchmark]
    public Result<bool> ValidateFromSpan()
    {
        return _validator.Validate(_validXmlBytes.AsSpan(), TargetNamespace);
    }

    /// <summary>
    /// Measures performance of checking schema presence in the cache.
    /// </summary>
    /// <returns><see langword="true"/> if the schema is registered; otherwise, <see langword="false"/>.</returns>
    [Benchmark]
    public bool CacheLookupContainsSchema()
    {
        return _cache.ContainsSchema(TargetNamespace);
    }
}
