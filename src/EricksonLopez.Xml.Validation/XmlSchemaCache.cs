// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace EricksonLopez.Xml.Validation;

/// <summary>
/// Provides a thread-safe cache for precompiled XSD schema sets with Anti-XXE protection.
/// </summary>
/// <remarks>
/// All schema compilation operations prohibit DTD processing and disable external entity resolution
/// to protect against XML External Entity (XXE) attacks and XML entity expansion vulnerabilities.
/// Thread safety is guaranteed for concurrent registration and lookup operations.
/// </remarks>
public sealed class XmlSchemaCache : IXmlSchemaCache, IXmlSchemaSetProvider
{
    private sealed record SchemaCacheEntry(XmlSchemaSet SchemaSet, HashSet<(string LocalName, string NamespaceUri)> GlobalElements);

    private readonly ConcurrentDictionary<string, SchemaCacheEntry> _cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaCache"/> class.
    /// </summary>
    public XmlSchemaCache() { }

    /// <inheritdoc/>
    public void RegisterSchema(string targetNamespace, string xsdContent)
    {
        ArgumentNullException.ThrowIfNull(targetNamespace);
        ArgumentNullException.ThrowIfNull(xsdContent);

        using var reader = new StringReader(xsdContent);
        using var xmlReader = XmlReader.Create(reader, CreateSafeReaderSettings());

        var schemaSet = new XmlSchemaSet { XmlResolver = null };
        schemaSet.Add(targetNamespace, xmlReader);
        schemaSet.Compile();

        CacheSchemaSet(targetNamespace, schemaSet);
    }

    /// <inheritdoc/>
    public void RegisterSchema(string targetNamespace, Stream xsdStream)
    {
        ArgumentNullException.ThrowIfNull(targetNamespace);
        ArgumentNullException.ThrowIfNull(xsdStream);

        using var xmlReader = XmlReader.Create(xsdStream, CreateSafeReaderSettings());

        var schemaSet = new XmlSchemaSet { XmlResolver = null };
        schemaSet.Add(targetNamespace, xmlReader);
        schemaSet.Compile();

        CacheSchemaSet(targetNamespace, schemaSet);
    }

    /// <inheritdoc/>
    public unsafe void RegisterSchema(string targetNamespace, ReadOnlySpan<byte> utf8Xsd)
    {
        ArgumentNullException.ThrowIfNull(targetNamespace);

        fixed (byte* ptr = utf8Xsd)
        {
            using var stream = ptr is null
                ? new MemoryStream(Array.Empty<byte>())
                : (Stream)new UnmanagedMemoryStream(ptr, utf8Xsd.Length, utf8Xsd.Length, FileAccess.Read);
            using var xmlReader = XmlReader.Create(stream, CreateSafeReaderSettings());

            var schemaSet = new XmlSchemaSet { XmlResolver = null };
            schemaSet.Add(targetNamespace, xmlReader);
            schemaSet.Compile();

            CacheSchemaSet(targetNamespace, schemaSet);
        }
    }


    bool IXmlSchemaSetProvider.TryGetSchemaSet(string targetNamespace, out XmlSchemaSet? schemaSet)
    {
        ArgumentNullException.ThrowIfNull(targetNamespace);
        if (_cache.TryGetValue(targetNamespace, out var entry))
        {
            schemaSet = entry.SchemaSet;
            return true;
        }

        schemaSet = null;
        return false;
    }

    /// <inheritdoc/>
    public bool ContainsSchema(string targetNamespace)
    {
        ArgumentNullException.ThrowIfNull(targetNamespace);
        return _cache.ContainsKey(targetNamespace);
    }

    /// <inheritdoc/>
    public bool IsRootElementDeclared(string targetNamespace, string localName, string namespaceUri)
    {
        ArgumentNullException.ThrowIfNull(targetNamespace);
        ArgumentNullException.ThrowIfNull(localName);
        ArgumentNullException.ThrowIfNull(namespaceUri);

        if (_cache.TryGetValue(targetNamespace, out var entry))
        {
            return entry.GlobalElements.Contains((localName, namespaceUri));
        }

        return false;
    }

    /// <inheritdoc/>
    public int Count => _cache.Count;

    /// <inheritdoc/>
    public void Clear()
    {
        _cache.Clear();
    }

    private void CacheSchemaSet(string targetNamespace, XmlSchemaSet schemaSet)
    {
        var elements = new HashSet<(string LocalName, string NamespaceUri)>();
        foreach (XmlSchemaElement element in schemaSet.GlobalElements.Values)
        {
            elements.Add((element.QualifiedName.Name, element.QualifiedName.Namespace));
        }

        _cache[targetNamespace] = new SchemaCacheEntry(schemaSet, elements);
    }

    internal void RegisterSchemaSetInternal(string targetNamespace, XmlSchemaSet schemaSet)
    {
        CacheSchemaSet(targetNamespace, schemaSet);
    }

    private static XmlReaderSettings CreateSafeReaderSettings() => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        CloseInput = false
    };
}
