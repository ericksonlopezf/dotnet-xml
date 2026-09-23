// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using AwesomeAssertions;
using EricksonLopez.Xml.Validation.Tests.Common;
using Xunit;
using static EricksonLopez.Xml.Validation.Tests.Common.XmlTestSamples;

namespace EricksonLopez.Xml.Validation.Tests;

/// <summary>
/// Verifies schema set compilation, caching, retrieval, and concurrency behavior of <see cref="XmlSchemaCache"/>.
/// </summary>
public sealed class XmlSchemaCacheTests
{

    // ─── RegisterSchema (string) ───────────────────────────────────────────────

    [Fact]
    public void RegisterSchema_String_RegistersAndCanRetrieve()
    {
        var cache = new XmlSchemaCache();

        cache.RegisterSchema(TargetNamespace, SampleXsd);

        cache.ContainsSchema(TargetNamespace).Should().BeTrue();
        cache.Count.Should().Be(1);
        cache.TryGetSchemaSet(TargetNamespace, out var schemaSet).Should().BeTrue();
        schemaSet.Should().NotBeNull();
        schemaSet!.IsCompiled.Should().BeTrue();
        schemaSet.GetXmlResolver().Should().BeNull();
        schemaSet.GlobalElements.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RegisterSchema_String_OverwritesSameNamespace()
    {
        var cache = new XmlSchemaCache();

        cache.RegisterSchema(TargetNamespace, SampleXsd);
        cache.RegisterSchema(TargetNamespace, SampleXsd); // Same NS, same XSD

        cache.Count.Should().Be(1); // Not 2 — overwrite, not accumulate
    }

    [Fact]
    public void RegisterSchema_TwoDifferentNamespaces_CountIsTwo()
    {
        var cache = new XmlSchemaCache();

        cache.RegisterSchema(TargetNamespace, SampleXsd);
        cache.RegisterSchema(AltNamespace, AltXsd);

        cache.Count.Should().Be(2);
    }

    [Fact]
    public void RegisterSchema_NullNamespace_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchema(null!, SampleXsd);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("targetNamespace");
    }

    [Fact]
    public void RegisterSchema_NullContent_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchema(TargetNamespace, (string)null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("xsdContent");
    }

    [Fact]
    public void RegisterSchema_InvalidXsd_ThrowsException()
    {
        var cache = new XmlSchemaCache();
        const string invalidXsd = "<not-a-schema>garbage</not-a-schema>";

        var act = () => cache.RegisterSchema(TargetNamespace, invalidXsd);

        act.Should().Throw<XmlSchemaException>();
    }

    // ─── RegisterSchema (Stream) ───────────────────────────────────────────────

    [Fact]
    public void RegisterSchema_Stream_RegistersSuccessfully()
    {
        var cache = new XmlSchemaCache();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(SampleXsd));
        cache.RegisterSchema(TargetNamespace, stream);

        cache.ContainsSchema(TargetNamespace).Should().BeTrue();
        cache.TryGetSchemaSet(TargetNamespace, out var schemaSet).Should().BeTrue();
        schemaSet.Should().NotBeNull();
        schemaSet!.IsCompiled.Should().BeTrue();
        schemaSet.GetXmlResolver().Should().BeNull();
        schemaSet.GlobalElements.Count.Should().BeGreaterThan(0);
        stream.CanRead.Should().BeTrue();
    }

    [Fact]
    public void RegisterSchema_NullStream_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchema(TargetNamespace, (Stream)null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("xsdStream");
    }

    [Fact]
    public void RegisterSchema_Stream_NullNamespace_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchema(null!, Stream.Null);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("targetNamespace");
    }

    // ─── RegisterSchema (ReadOnlySpan<byte>) ──────────────────────────────────

    [Fact]
    public void RegisterSchema_Utf8Span_RegistersSuccessfully()
    {
        var cache = new XmlSchemaCache();
        var xsdBytes = Encoding.UTF8.GetBytes(SampleXsd);

        cache.RegisterSchema(TargetNamespace, xsdBytes.AsSpan());

        cache.ContainsSchema(TargetNamespace).Should().BeTrue();
        cache.Count.Should().Be(1);
        cache.TryGetSchemaSet(TargetNamespace, out var schemaSet).Should().BeTrue();
        schemaSet.Should().NotBeNull();
        schemaSet!.IsCompiled.Should().BeTrue();
        schemaSet.GetXmlResolver().Should().BeNull();
        schemaSet.GlobalElements.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RegisterSchema_Utf8Span_NullNamespace_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();
        try
        {
            cache.RegisterSchema(null!, ReadOnlySpan<byte>.Empty);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            ex.ParamName.Should().Be("targetNamespace");
        }
    }

    [Fact]
    public void RegisterSchema_Utf8Span_EmptySpan_ThrowsXmlException()
    {
        var cache = new XmlSchemaCache();
        try
        {
            cache.RegisterSchema(TargetNamespace, ReadOnlySpan<byte>.Empty);
            Assert.Fail("Expected XmlException for empty span");
        }
        catch (XmlException)
        {
            // Expected: Empty span causes underlying reader to throw XmlException as documented in IXmlSchemaCache
        }
    }

    // ─── RegisterSchemaFile ───────────────────────────────────────────────────

    [Fact]
    public void RegisterSchemaFile_NonExistentFile_ThrowsFileNotFoundException()
    {
        var cache = new XmlSchemaCache();

        const string path = "non_existent_file.xsd";
        var act = () => cache.RegisterSchemaFile(TargetNamespace, path);

        var ex = act.Should().Throw<FileNotFoundException>().Which;
        ex.FileName.Should().Be(path);
        ex.Message.Should().Contain(path);
    }

    [Fact]
    public void RegisterSchemaFile_ValidFile_RegistersSuccessfully()
    {
        var tempFile = Path.GetTempFileName() + ".xsd";
        try
        {
            File.WriteAllText(tempFile, SampleXsd, Encoding.UTF8);
            var cache = new XmlSchemaCache();

            cache.RegisterSchemaFile(TargetNamespace, tempFile);

            cache.ContainsSchema(TargetNamespace).Should().BeTrue();
            cache.TryGetSchemaSet(TargetNamespace, out var schemaSet).Should().BeTrue();
            schemaSet.Should().NotBeNull();
            schemaSet.GetXmlResolver().Should().BeNull();
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void RegisterSchemaFile_NullTargetNamespace_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchemaFile(null!, "some_path.xsd");

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("targetNamespace");
    }

    [Fact]
    public void RegisterSchemaFile_NullTargetNamespace_WithExistingFile_ThrowsArgumentNullException()
    {
        var tempFile = Path.GetTempFileName() + ".xsd";
        try
        {
            File.WriteAllText(tempFile, SampleXsd, Encoding.UTF8);
            var cache = new XmlSchemaCache();

            var act = () => cache.RegisterSchemaFile(null!, tempFile);

            var ex = act.Should().Throw<ArgumentNullException>().Which;
            ex.ParamName.Should().Be("targetNamespace");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void RegisterSchemaFile_NullFilePath_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchemaFile(TargetNamespace, null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("filePath");
    }

    [Fact]
    public void RegisterSchemaFile_NullCache_ThrowsArgumentNullException()
    {
        var act = () => XmlSchemaCacheExtensions.RegisterSchemaFile(null!, TargetNamespace, "some_path.xsd");

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("cache");
    }

    [Fact]
    public void RegisterSchemasFromDirectory_NullCache_ThrowsArgumentNullException()
    {
        var act = () => XmlSchemaCacheExtensions.RegisterSchemasFromDirectory(null!, ".");

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("cache");
    }

    [Fact]
    public void RegisterSchemasFromDirectory_NullDirectory_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchemasFromDirectory(null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("directoryPath");
    }

    [Fact]
    public void RegisterSchemasFromDirectory_NullSearchPattern_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchemasFromDirectory(".", null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("searchPattern");
    }

    [Fact]
    public void RegisterSchemasFromDirectory_NullSearchPattern_WithNonExistentDirectory_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchemasFromDirectory("non_existent_dir_12345", null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("searchPattern");
    }

    // ─── RegisterSchemasFromDirectory ─────────────────────────────────────────

    [Fact]
    public void RegisterSchemasFromDirectory_NonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        var cache = new XmlSchemaCache();

        const string dir = "non_existent_dir_12345";
        var act = () => cache.RegisterSchemasFromDirectory(dir);

        var ex = act.Should().Throw<DirectoryNotFoundException>().Which;
        ex.Message.Should().StartWith("XSD schema directory not found at path");
    }

    [Fact]
    public void RegisterSchemasFromDirectory_TempDirectoryWithXsds_RegistersAll()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_Test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var filePath = Path.Combine(tempDir, "Invoice.xsd");
            File.WriteAllText(filePath, SampleXsd, Encoding.UTF8);

            var cache = new XmlSchemaCache();
            var count = cache.RegisterSchemasFromDirectory(tempDir);

            count.Should().Be(1);
            cache.ContainsSchema(TargetNamespace).Should().BeTrue();
            cache.TryGetSchemaSet(TargetNamespace, out var dirSet).Should().BeTrue();
            dirSet.Should().NotBeNull();
            dirSet.GetXmlResolver().Should().BeNull();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void RegisterSchema_WithExternalSchemaImport_DoesNotResolveExternalUris()
    {
        // An XSD attempting to import an external URI via schemaLocation
        const string xsdWithExternalImport = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/import-test"
                       xmlns:ext="http://external.org/test"
                       elementFormDefault="qualified">
              <xs:import namespace="http://external.org/test" schemaLocation="http://127.0.0.1:9999/nonexistent.xsd" />
              <xs:element name="Root" type="xs:string" />
            </xs:schema>
            """;

        var cache = new XmlSchemaCache();
        // With XmlResolver = null, external URIs are not resolved and compilation succeeds safely without network calls
        cache.RegisterSchema("http://ericksonlopez.dev/import-test", xsdWithExternalImport);

        cache.ContainsSchema("http://ericksonlopez.dev/import-test").Should().BeTrue();
    }

    [Fact]
    public void RegisterSchemasFromDirectory_MalformedXsd_TriggersCompilationErrorCallback()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_Test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var filePath = Path.Combine(tempDir, "Broken.xsd");
            const string brokenXsd = """
                <?xml version="1.0" encoding="utf-8"?>
                <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" targetNamespace="http://test">
                  <xs:invalidElementNotAllowedHere />
                </xs:schema>
                """;
            File.WriteAllText(filePath, brokenXsd, Encoding.UTF8);

            var cache = new XmlSchemaCache();
            var act = () => cache.RegisterSchemasFromDirectory(tempDir);

            var ex = act.Should().Throw<XmlSchemaException>().Which;
            ex.Message.Should().Contain("Broken.xsd");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void RegisterSchemasFromDirectory_EmptyDirectory_ReturnsZero()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_Empty_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var cache = new XmlSchemaCache();
            var count = cache.RegisterSchemasFromDirectory(tempDir);

            count.Should().Be(0);
            cache.Count.Should().Be(0);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void RegisterSchemasFromDirectory_NullPath_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.RegisterSchemasFromDirectory(null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("directoryPath");
    }

    [Fact]
    public void RegisterSchemasFromDirectory_WithUnresolvedExternalImportType_ThrowsXmlSchemaException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_Interdep_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "01_Common.xsd"), CommonTypesXsd, Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDir, "02_Order.xsd"), OrderWithImportXsd, Encoding.UTF8);

            var cache = new XmlSchemaCache();
            // OrderWithImportXsd references c:CustomerType which cannot be resolved via external schemaLocation
            // because XmlResolver is null by design for Anti-XXE security.
            var act = () => cache.RegisterSchemasFromDirectory(tempDir);

            var ex = act.Should().Throw<XmlSchemaException>().Which;
            ex.Message.Should().Contain("CustomerType");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void RegisterSchemasFromDirectory_WithExternalSchemaImport_DoesNotResolveExternalNetworkUris()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_ExtDir_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "ExtImport.xsd"), ExternalDirImportXsd, Encoding.UTF8);

            var cache = new XmlSchemaCache();
            var count = cache.RegisterSchemasFromDirectory(tempDir);

            count.Should().Be(1);
            cache.ContainsSchema(ExternalDirImportNamespace).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void RegisterSchemasFromDirectory_MultipleValidSchemas_ReturnsExactCountAndRegistersEach()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_Multi_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Invoice.xsd"), SampleXsd, Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDir, "Product.xsd"), AltXsd, Encoding.UTF8);

            var cache = new XmlSchemaCache();
            var count = cache.RegisterSchemasFromDirectory(tempDir);

            count.Should().Be(2);
            cache.Count.Should().Be(2);
            cache.ContainsSchema(TargetNamespace).Should().BeTrue();
            cache.ContainsSchema(AltNamespace).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    // ─── TryGetSchemaSet ──────────────────────────────────────────────────────

    [Fact]
    public void TryGetSchemaSet_RegisteredSchema_ReturnsTrueAndSchemaSet()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var found = cache.TryGetSchemaSet(TargetNamespace, out var schemaSet);

        found.Should().BeTrue();
        schemaSet.Should().NotBeNull();
    }

    [Fact]
    public void TryGetSchemaSet_UnregisteredSchema_ReturnsFalseAndNull()
    {
        var cache = new XmlSchemaCache();

        var found = cache.TryGetSchemaSet("http://not.registered", out var schemaSet);

        found.Should().BeFalse();
        schemaSet.Should().BeNull();
    }

    [Fact]
    public void TryGetSchemaSet_NullNamespace_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.TryGetSchemaSet(null!, out _);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("targetNamespace");
    }

    // ─── ContainsSchema ───────────────────────────────────────────────────────

    [Fact]
    public void ContainsSchema_RegisteredNamespace_ReturnsTrue()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        cache.ContainsSchema(TargetNamespace).Should().BeTrue();
    }

    [Fact]
    public void ContainsSchema_UnregisteredNamespace_ReturnsFalse()
    {
        var cache = new XmlSchemaCache();

        cache.ContainsSchema("http://not.registered").Should().BeFalse();
    }

    [Fact]
    public void ContainsSchema_NullNamespace_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.ContainsSchema(null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("targetNamespace");
    }

    // ─── Count & Clear ────────────────────────────────────────────────────────

    [Fact]
    public void Count_EmptyCache_IsZero()
    {
        var cache = new XmlSchemaCache();
        cache.Count.Should().Be(0);
    }

    [Fact]
    public void Clear_RegisteredSchemas_EmptiesCacheAndResetsCount()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        cache.Count.Should().Be(1);

        cache.Clear();
        cache.Count.Should().Be(0);
        cache.ContainsSchema(TargetNamespace).Should().BeFalse();
    }

    [Fact]
    public void Clear_EmptyCache_DoesNotThrow()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.Clear();

        act.Should().NotThrow();
    }

    // ─── Thread-Safety (Concurrency) ──────────────────────────────────────────

    [Fact]
    public void RegisterSchema_ConcurrentReads_AllSucceed()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        cache.RegisterSchema(AltNamespace, AltXsd);

        const int threadCount = 50;
        var results = new bool[threadCount];
        var threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            var idx = i;
            threads[idx] = new Thread(() =>
            {
                var ns = idx % 2 == 0 ? TargetNamespace : AltNamespace;
                results[idx] = cache.ContainsSchema(ns);
            });
        }

        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();

        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public void RegisterSchema_ConcurrentWrites_NoExceptionAndCountConsistent()
    {
        const int threadCount = 20;
        var cache = new XmlSchemaCache();
        var exceptions = new List<Exception>();
        var threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            var idx = i;
            threads[idx] = new Thread(() =>
            {
                try
                {
                    var ns = $"http://ericksonlopez.dev/ns{idx}";
                    cache.RegisterSchema(ns, SampleXsd.Replace(TargetNamespace, ns));
                }
                catch (Exception ex)
                {
                    lock (exceptions) exceptions.Add(ex);
                }
            });
        }

        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();

        exceptions.Should().BeEmpty();
        cache.Count.Should().Be(threadCount);
    }

    [Fact]
    public async Task RegisterSchema_ConcurrentReadWriteMix_NoExceptionAndNoCorruption()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        const int concurrency = 30;
        var exceptions = new List<Exception>();
        var tasks = new List<Task>();

        // 15 writers, 15 readers running simultaneously
        for (int i = 0; i < concurrency; i++)
        {
            var idx = i;
            if (idx % 2 == 0)
            {
                // Writer
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var ns = $"http://ericksonlopez.dev/concurrent{idx}";
                        cache.RegisterSchema(ns, SampleXsd.Replace(TargetNamespace, ns));
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions) exceptions.Add(ex);
                    }
                }));
            }
            else
            {
                // Reader
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        _ = cache.ContainsSchema(TargetNamespace);
                        _ = cache.Count;
                        _ = cache.TryGetSchemaSet(TargetNamespace, out _);
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions) exceptions.Add(ex);
                    }
                }));
            }
        }

        await Task.WhenAll(tasks);

        exceptions.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ConcurrentValidations_AllReturnConsistentResults()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        const string validXml = """
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>INV-001</Id>
                <Total>199.99</Total>
            </Invoice>
            """;

        const int taskCount = 20;
        var tasks = new Task<bool>[taskCount];

        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(validXml));
                var result = await validator.ValidateAsync(stream, TargetNamespace);
                return result.IsSuccess;
            });
        }

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public void Validate_ConcurrentValidations_AllReturnConsistentResults()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        var validator = new XmlSchemaValidator(cache);

        const string validXml = """
            <Invoice xmlns="http://ericksonlopez.dev/invoice">
                <Id>INV-001</Id>
                <Total>199.99</Total>
            </Invoice>
            """;

        const int threadCount = 20;
        var results = new bool[threadCount];
        var threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            var idx = i;
            threads[idx] = new Thread(() =>
            {
                var r = validator.Validate(validXml, TargetNamespace);
                results[idx] = r.IsSuccess;
            });
        }

        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();

        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }


    [Fact]
    public void RegisterSchemasFromDirectory_SchemaWithoutTargetNamespace_RegistersWithEmptyNamespace()
    {
        var cache = new XmlSchemaCache();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        const string noNamespaceXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
              <xs:element name="Item" type="xs:string" />
            </xs:schema>
            """;

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "item.xsd"), noNamespaceXsd);

            var count = cache.RegisterSchemasFromDirectory(tempDir);
            count.Should().Be(1);

            cache.ContainsSchema(string.Empty).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void RegisterSchemasFromDirectory_InvalidSchemaFile_ThrowsXmlSchemaException()
    {
        var cache = new XmlSchemaCache();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        const string corruptXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
              <xs:element name="Broken" type="xs:invalidTypeNonExistent" />
            </xs:schema>
            """;

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "corrupt.xsd"), corruptXsd);

            var act = () => cache.RegisterSchemasFromDirectory(tempDir);
            act.Should().Throw<XmlSchemaException>();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ContainsSchema_ReturnsFalseForUnregisteredNamespace()
    {
        var cache = new XmlSchemaCache();
        cache.ContainsSchema("http://not-registered.org").Should().BeFalse();
    }

    [Fact]
    public void XmlSchemaTestHelper_CorrectlyDetectsResolverConfiguration()
    {
        var unconfiguredSet = new XmlSchemaSet();
        unconfiguredSet.GetXmlResolver().Should().NotBeNull();

        var safeSet = new XmlSchemaSet { XmlResolver = null };
        safeSet.GetXmlResolver().Should().BeNull();
    }

    [Fact]
    public void RegisterSchemasFromDirectory_MultipleFilesSameNamespace_CompilesAllIntoSingleSet()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_Modular_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        const string crmNs = "http://ericksonlopez.dev/crm";
        const string typesXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/crm"
                       xmlns:crm="http://ericksonlopez.dev/crm"
                       elementFormDefault="qualified">
              <xs:complexType name="AddressType">
                <xs:sequence>
                  <xs:element name="City" type="xs:string" />
                  <xs:element name="Zip" type="xs:string" />
                </xs:sequence>
              </xs:complexType>
            </xs:schema>
            """;

        const string customerXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/crm"
                       xmlns:crm="http://ericksonlopez.dev/crm"
                       elementFormDefault="qualified">
              <xs:element name="Customer">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name="Name" type="xs:string" />
                    <xs:element name="Address" type="crm:AddressType" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>
            """;

        const string validCustomerXml = """
            <Customer xmlns="http://ericksonlopez.dev/crm">
              <Name>Enterprise Customer</Name>
              <Address>
                <City>Austin</City>
                <Zip>78701</Zip>
              </Address>
            </Customer>
            """;

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "01_types.xsd"), typesXsd, Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDir, "02_customer.xsd"), customerXsd, Encoding.UTF8);

            var cache = new XmlSchemaCache();
            var registeredCount = cache.RegisterSchemasFromDirectory(tempDir);

            registeredCount.Should().Be(2, "Both modular schema files should be read and registered.");
            cache.Count.Should().Be(1, "Both files belong to the same targetNamespace and should form a single compiled schema set.");
            cache.ContainsSchema(crmNs).Should().BeTrue();

            var validator = new XmlSchemaValidator(cache);
            var result = validator.Validate(validCustomerXml, crmNs);
            result.IsSuccess.Should().BeTrue("The compiled set must include types from both modular XSD files.");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    // ─── IsRootElementDeclared & Defensive Resolver Neutralization ────────────

    [Fact]
    public void IsRootElementDeclared_DeclaredElement_ReturnsTrue()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var result = cache.IsRootElementDeclared(TargetNamespace, "Invoice", TargetNamespace);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsRootElementDeclared_UndeclaredElement_ReturnsFalse()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var result = cache.IsRootElementDeclared(TargetNamespace, "NonExistentElement", TargetNamespace);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRootElementDeclared_WrongNamespace_ReturnsFalse()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);

        var result = cache.IsRootElementDeclared(TargetNamespace, "Invoice", "http://wrong.namespace/crm");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRootElementDeclared_UnregisteredTargetNamespace_ReturnsFalse()
    {
        var cache = new XmlSchemaCache();

        var result = cache.IsRootElementDeclared("http://unregistered.namespace", "Invoice", TargetNamespace);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRootElementDeclared_NullTargetNamespace_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.IsRootElementDeclared(null!, "Invoice", TargetNamespace);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("targetNamespace");
    }

    [Fact]
    public void IsRootElementDeclared_NullLocalName_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.IsRootElementDeclared(TargetNamespace, null!, TargetNamespace);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("localName");
    }

    [Fact]
    public void IsRootElementDeclared_NullNamespaceUri_ThrowsArgumentNullException()
    {
        var cache = new XmlSchemaCache();

        var act = () => cache.IsRootElementDeclared(TargetNamespace, "Invoice", null!);

        var ex = act.Should().Throw<ArgumentNullException>().Which;
        ex.ParamName.Should().Be("namespaceUri");
    }

    [Fact]
    public void IsRootElementDeclared_AfterClear_ReturnsFalse()
    {
        var cache = new XmlSchemaCache();
        cache.RegisterSchema(TargetNamespace, SampleXsd);
        cache.IsRootElementDeclared(TargetNamespace, "Invoice", TargetNamespace).Should().BeTrue();

        cache.Clear();

        cache.IsRootElementDeclared(TargetNamespace, "Invoice", TargetNamespace).Should().BeFalse();
    }

    [Fact]
    public void RegisterSchemasFromDirectory_SortsFilesAlphabetically_BeforeCompiling()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_Sort_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            const string sortNs = "http://ericksonlopez.dev/sort-test";
            // In Windows NTFS case-insensitive B-Tree order, 'a_schema.xsd' precedes 'B_schema.xsd'.
            // In StringComparer.Ordinal sort order, 'B' (0x42) strictly precedes 'a' (0x61).
            var fileLowerA = Path.Combine(tempDir, "a_schema.xsd");
            var fileUpperB = Path.Combine(tempDir, "B_schema.xsd");

            File.WriteAllText(fileLowerA, $"""
                <?xml version="1.0" encoding="utf-8"?>
                <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" targetNamespace="{sortNs}">
                  <xs:element name="AElement" type="xs:string" />
                </xs:schema>
                """);

            File.WriteAllText(fileUpperB, $"""
                <?xml version="1.0" encoding="utf-8"?>
                <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" targetNamespace="{sortNs}">
                  <xs:element name="BElement" type="xs:string" />
                </xs:schema>
                """);

            var cache = new XmlSchemaCache();
            cache.RegisterSchemasFromDirectory(tempDir);

            cache.TryGetSchemaSet(sortNs, out var schemaSet).Should().BeTrue();
            var schemas = schemaSet!.Schemas(sortNs).Cast<XmlSchema>().ToList();
            schemas.Should().HaveCount(2);
            schemas[0].SourceUri.Should().Be(fileUpperB);
            schemas[1].SourceUri.Should().Be(fileLowerA);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void RegisterSchemasFromDirectory_WithCustomSchemaCache_InvokesLspFallbackAndRegistersEach()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_CustomCache_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        const string crmNs = "http://ericksonlopez.dev/crm";
        const string typesXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/crm"
                       elementFormDefault="qualified">
              <xs:complexType name="AddressType">
                <xs:sequence>
                  <xs:element name="City" type="xs:string" />
                </xs:sequence>
              </xs:complexType>
            </xs:schema>
            """;

        const string customerXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                       targetNamespace="http://ericksonlopez.dev/crm"
                       elementFormDefault="qualified">
              <xs:element name="Customer" type="xs:string" />
            </xs:schema>
            """;

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "01_types.xsd"), typesXsd, Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDir, "02_customer.xsd"), customerXsd, Encoding.UTF8);

            var customCache = new MockCustomSchemaCache();
            var count = customCache.RegisterSchemasFromDirectory(tempDir);

            count.Should().Be(2);
            customCache.RegisteredSchemas.Should().HaveCount(2);
            customCache.RegisteredSchemas[0].TargetNamespace.Should().Be(crmNs);
            customCache.RegisteredSchemas[0].Content.Should().Contain("AddressType");
            customCache.RegisteredSchemas[1].TargetNamespace.Should().Be(crmNs);
            customCache.RegisteredSchemas[1].Content.Should().Contain("Customer");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void RegisterSchemasFromDirectory_WithCustomSchemaCache_MultipleNamespaces_RegistersAll()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_CustomMultiNs_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "01_main.xsd"), SampleXsd, Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDir, "02_alt.xsd"), AltXsd, Encoding.UTF8);

            var customCache = new MockCustomSchemaCache();
            var count = customCache.RegisterSchemasFromDirectory(tempDir);

            count.Should().Be(2);
            customCache.RegisteredSchemas.Should().HaveCount(2);
            customCache.RegisteredSchemas.Select(s => s.TargetNamespace).Should().Contain([TargetNamespace, AltNamespace]);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void RegisterSchemasFromDirectory_FileWithDtd_ThrowsXmlException()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_DtdXsd_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        const string dtdXsd = """
            <?xml version="1.0" encoding="utf-8"?>
            <!DOCTYPE xs:schema [<!ENTITY ext "test">]>
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" targetNamespace="http://example.com">
              <xs:element name="Root" type="xs:string" />
            </xs:schema>
            """;

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "dtd.xsd"), dtdXsd, Encoding.UTF8);
            var cache = new XmlSchemaCache();
            var act = () => cache.RegisterSchemasFromDirectory(tempDir);
            act.Should().Throw<XmlException>();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void RegisterSchemasFromDirectory_NonSchemaXmlFile_ThrowsXmlSchemaException()
    {
        var cache = new XmlSchemaCache();
        var tempDir = Path.Combine(Path.GetTempPath(), "EL_XmlValidation_NonSchemaXml_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        const string notASchemaXml = """
            <?xml version="1.0" encoding="utf-8"?>
            <Root>
              <Child>This is valid XML but not an XML schema</Child>
            </Root>
            """;

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "not_schema.xsd"), notASchemaXml, Encoding.UTF8);

            var act = () => cache.RegisterSchemasFromDirectory(tempDir);
            act.Should().Throw<XmlSchemaException>();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private sealed class MockCustomSchemaCache : IXmlSchemaCache
    {
        public List<(string TargetNamespace, string Content)> RegisteredSchemas { get; } = new();

        public void RegisterSchema(string targetNamespace, string xsdContent)
        {
            RegisteredSchemas.Add((targetNamespace, xsdContent));
        }

        public void RegisterSchema(string targetNamespace, Stream xsdStream)
        {
            using var reader = new StreamReader(xsdStream, Encoding.UTF8, leaveOpen: true);
            RegisteredSchemas.Add((targetNamespace, reader.ReadToEnd()));
        }

        public void RegisterSchema(string targetNamespace, ReadOnlySpan<byte> utf8Xsd)
        {
            RegisteredSchemas.Add((targetNamespace, Encoding.UTF8.GetString(utf8Xsd)));
        }

        public bool ContainsSchema(string targetNamespace) => RegisteredSchemas.Exists(s => s.TargetNamespace == targetNamespace);

        public int Count => RegisteredSchemas.Count;

        public bool IsRootElementDeclared(string targetNamespace, string localName, string namespaceUri) => true;

        public void Clear() => RegisteredSchemas.Clear();
    }
}
