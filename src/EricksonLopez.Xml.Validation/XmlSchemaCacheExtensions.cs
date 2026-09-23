// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace EricksonLopez.Xml.Validation;

/// <summary>
/// Provides extension methods for <see cref="IXmlSchemaCache"/> to support
/// registration of XML schemas from the file system.
/// </summary>
public static class XmlSchemaCacheExtensions
{
    /// <summary>
    /// Registers a precompiled XSD schema definition from a file on disk.
    /// </summary>
    /// <param name="cache">The schema cache in which to register the schema</param>
    /// <param name="targetNamespace">The target XML namespace associated with the schema</param>
    /// <param name="filePath">The file path to the XSD schema file to compile and register</param>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/>, <paramref name="targetNamespace"/>, or <paramref name="filePath"/> is <see langword="null"/></exception>
    /// <exception cref="FileNotFoundException">The file specified by <paramref name="filePath"/> was not found</exception>
    /// <exception cref="XmlException">The schema file does not contain well-formed XML</exception>
    /// <exception cref="XmlSchemaException">The schema contains structural or syntax errors</exception>
    public static void RegisterSchemaFile(this IXmlSchemaCache cache, string targetNamespace, string filePath)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(targetNamespace);
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"XSD schema file not found at path '{filePath}'.", filePath);
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        cache.RegisterSchema(targetNamespace, fileStream);
    }

    /// <summary>
    /// Registers all matching XSD schema files discovered within the specified directory.
    /// </summary>
    /// <param name="cache">The schema cache in which to register the schemas</param>
    /// <param name="directoryPath">The directory path containing XSD schema files</param>
    /// <param name="searchPattern">The search pattern used to match schema files. Defaults to "*.xsd".</param>
    /// <returns>
    /// The number of schemas successfully compiled and registered. Returns <c>0</c> if no files match <paramref name="searchPattern"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="cache"/>, <paramref name="directoryPath"/>, or <paramref name="searchPattern"/> is <see langword="null"/></exception>
    /// <exception cref="DirectoryNotFoundException">The directory specified by <paramref name="directoryPath"/> does not exist</exception>
    /// <exception cref="XmlException">A schema file does not contain well-formed XML</exception>
    /// <exception cref="XmlSchemaException">A schema file contains XML or schema compilation errors</exception>
    /// <remarks>
    /// <para>
    /// <b>Optimized path (<see cref="XmlSchemaCache"/>):</b>
    /// When <paramref name="cache"/> is the built-in <see cref="XmlSchemaCache"/>, all XSD files that share
    /// the same <c>targetNamespace</c> are merged into a single compiled <see cref="System.Xml.Schema.XmlSchemaSet"/>
    /// before insertion. This ensures that schemas split across multiple files (e.g., via <c>xs:include</c>) are
    /// correctly aggregated into one cache entry per namespace.
    /// </para>
    /// <para>
    /// <b>Fallback path (custom <see cref="IXmlSchemaCache"/> implementations):</b>
    /// When <paramref name="cache"/> is a custom implementation that does not extend <see cref="XmlSchemaCache"/>,
    /// a Liskov Substitution Principle (LSP) compatible fallback is used: each file is re-registered individually
    /// via <see cref="IXmlSchemaCache.RegisterSchema(string, System.IO.Stream)"/>. If multiple files share the
    /// same <c>targetNamespace</c>, each call overwrites the previous entry (per ADR-003). Only the last file
    /// for a given namespace will be retained. Custom implementations of <see cref="IXmlSchemaCache"/> that
    /// need to support multi-file namespace aggregation must implement their own merge logic.
    /// </para>
    /// <para>
    /// Anti-XXE settings (<c>DtdProcessing.Prohibit</c>, <c>XmlResolver = null</c>) are enforced during
    /// all file reading and schema compilation operations in both paths.
    /// </para>
    /// </remarks>
    public static int RegisterSchemasFromDirectory(this IXmlSchemaCache cache, string directoryPath, string searchPattern = "*.xsd")
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(directoryPath);
        ArgumentNullException.ThrowIfNull(searchPattern);

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"XSD schema directory not found at path '{directoryPath}'.");
        }

        var files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
        Array.Sort(files, StringComparer.Ordinal);
        var schemasByNamespace = new Dictionary<string, List<XmlSchema>>(StringComparer.Ordinal);
        var registeredCount = 0;

        foreach (var file in files)
        {
            var schema = ReadSchemaFromFile(file);

            var targetNs = schema.TargetNamespace ?? string.Empty;
            if (!schemasByNamespace.TryGetValue(targetNs, out var list))
            {
                list = new List<XmlSchema>();
                schemasByNamespace[targetNs] = list;
            }

            list.Add(schema);
            registeredCount++;
        }

        foreach (var (targetNs, schemaList) in schemasByNamespace)
        {
            RegisterCompiledSchemaGroup(cache, targetNs, schemaList);
        }

        return registeredCount;
    }

    private static XmlSchema ReadSchemaFromFile(string file)
    {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var xmlReader = XmlReader.Create(fileStream, CreateSafeReaderSettings());

        var schema = XmlSchema.Read(xmlReader, (_, args) =>
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                throw new XmlSchemaException($"Schema compilation error in '{file}': {args.Message}", args.Exception);
            }
        })!;

        schema.SourceUri = file;
        return schema;
    }

    private static XmlReaderSettings CreateSafeReaderSettings() => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null
    };

    private static void RegisterCompiledSchemaGroup(IXmlSchemaCache cache, string targetNs, List<XmlSchema> schemaList)
    {
        if (cache is XmlSchemaCache concreteCache)
        {
            var schemaSet = new XmlSchemaSet { XmlResolver = null };
            foreach (var schema in schemaList)
            {
                schemaSet.Add(schema);
            }

            schemaSet.Compile();
            concreteCache.RegisterSchemaSetInternal(targetNs, schemaSet);
            return;
        }

        // LSP Fallback for custom implementations:
        // Re-read schemas from their original source files to avoid AOT-incompatible XmlSchema.Write()
        foreach (var schema in schemaList)
        {
            using var fileStream = new FileStream(schema.SourceUri!, FileMode.Open, FileAccess.Read, FileShare.Read);
            cache.RegisterSchema(targetNs, fileStream);
        }
    }
}
