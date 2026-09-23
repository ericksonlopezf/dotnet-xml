// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Text;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Xml.Validation.Tests.Common;
using Xunit;

namespace EricksonLopez.Xml.Validation.Tests;

/// <summary>
/// Property-based testing and mutation fuzzing engine for EricksonLopez.Xml.Validation.
/// Verifies determinism, modality equivalence (string vs stream vs span), and crash-resilience.
/// </summary>
public sealed class FuzzingAndPropertyTests
{
    private readonly XmlSchemaCache _cache;
    private readonly XmlSchemaValidator _validator;

    public FuzzingAndPropertyTests()
    {
        _cache = new XmlSchemaCache();
        _cache.RegisterSchema(XmlTestSamples.TargetNamespace, XmlTestSamples.SampleXsd);
        _validator = new XmlSchemaValidator(_cache);
    }

    [Theory]
    [InlineData(XmlTestSamples.ValidXml)]
    [InlineData(XmlTestSamples.InvalidXml)]
    [InlineData(XmlTestSamples.XxeXml)]
    public void Property_Determinism_ProducesIdenticalResultsAcrossIterations(string xml)
    {
        var baseline = _validator.Validate(xml, XmlTestSamples.TargetNamespace);

        for (int i = 0; i < 50; i++)
        {
            var current = _validator.Validate(xml, XmlTestSamples.TargetNamespace);
            current.IsSuccess.Should().Be(baseline.IsSuccess);
            if (baseline.IsFailure)
            {
                current.Error.Code.Should().Be(baseline.Error.Code);
                current.Error.Description.Should().Be(baseline.Error.Description);
            }
        }
    }

    [Theory]
    [InlineData(XmlTestSamples.ValidXml)]
    [InlineData(XmlTestSamples.InvalidXml)]
    [InlineData(XmlTestSamples.XxeXml)]
    public void Property_ModalityEquivalence_StringAndSpanAndStreamYieldSameOutcome(string xml)
    {
        var stringResult = _validator.Validate(xml, XmlTestSamples.TargetNamespace);

        var utf8Bytes = Encoding.UTF8.GetBytes(xml);
        var spanResult = _validator.Validate(utf8Bytes.AsSpan(), XmlTestSamples.TargetNamespace);

        using var stream = new MemoryStream(utf8Bytes);
        var streamResult = _validator.Validate(stream, XmlTestSamples.TargetNamespace);

        spanResult.IsSuccess.Should().Be(stringResult.IsSuccess);
        streamResult.IsSuccess.Should().Be(stringResult.IsSuccess);

        if (stringResult.IsFailure)
        {
            spanResult.Error.Code.Should().Be(stringResult.Error.Code);
            streamResult.Error.Code.Should().Be(stringResult.Error.Code);
        }
    }

    [Fact]
    public void Fuzzing_MutatedByteSequences_NeverThrowUnhandledExceptions()
    {
        var rng = new Random(42);
        var seedBytes = Encoding.UTF8.GetBytes(XmlTestSamples.ValidXml);

        for (int iteration = 0; iteration < 200; iteration++)
        {
            var mutated = (byte[])seedBytes.Clone();
            int mutationCount = rng.Next(1, 15);

            for (int m = 0; m < mutationCount; m++)
            {
                int op = rng.Next(3);
                int pos = rng.Next(mutated.Length);

                switch (op)
                {
                    case 0: // Bit flip
                        mutated[pos] ^= (byte)(1 << rng.Next(8));
                        break;
                    case 1: // Replace with random byte (including control chars & 0)
                        mutated[pos] = (byte)rng.Next(256);
                        break;
                    case 2: // Insert XML delimiter
                        mutated[pos] = (byte)"<>\"'&/="[rng.Next(7)];
                        break;
                }
            }

            // Invariant: validator must NEVER throw unhandled exception on malformed bytes
            Action act = () =>
            {
                var result = _validator.Validate(mutated.AsSpan(), XmlTestSamples.TargetNamespace);
                // Either success or structured failure
                (result.IsSuccess || result.IsFailure).Should().BeTrue();
            };

            act.Should().NotThrow();
        }
    }
}
