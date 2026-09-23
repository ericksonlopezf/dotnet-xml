// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace EricksonLopez.Xml.Validation;

/// <summary>
/// Defines a thread-safe registry and cache for precompiled XSD schema sets.
/// </summary>
/// <remarks>
/// Implementations must be thread-safe for concurrent registration and lookup operations.
/// Precompiled <see cref="XmlSchemaSet"/> instances are cached to eliminate redundant compilation overhead
/// and ensure safe schema resolution with external entity resolution disabled.
/// </remarks>
public interface IXmlSchemaCache
{
    /// <summary>
    /// Registers a precompiled XSD schema definition from the specified string.
    /// </summary>
    /// <param name="targetNamespace">The target XML namespace associated with the schema</param>
    /// <param name="xsdContent">The raw XML Schema Definition (XSD) string to compile</param>
    /// <exception cref="ArgumentNullException"><paramref name="targetNamespace"/> or <paramref name="xsdContent"/> is <see langword="null"/></exception>
    /// <exception cref="XmlException">The <paramref name="xsdContent"/> is not well-formed XML</exception>
    /// <exception cref="XmlSchemaException">The <paramref name="xsdContent"/> contains schema structural or syntax errors</exception>
    void RegisterSchema(string targetNamespace, string xsdContent);

    /// <summary>
    /// Registers a precompiled XSD schema definition from the specified stream.
    /// </summary>
    /// <param name="targetNamespace">The target XML namespace associated with the schema</param>
    /// <param name="xsdStream">The readable stream containing XSD schema data</param>
    /// <exception cref="ArgumentNullException"><paramref name="targetNamespace"/> or <paramref name="xsdStream"/> is <see langword="null"/></exception>
    /// <exception cref="XmlException">The stream does not contain well-formed XML</exception>
    /// <exception cref="XmlSchemaException">The stream contains schema structural or syntax errors</exception>
    void RegisterSchema(string targetNamespace, Stream xsdStream);

    /// <summary>
    /// Registers a precompiled XSD schema definition from the specified UTF-8 byte span.
    /// </summary>
    /// <param name="targetNamespace">The target XML namespace associated with the schema</param>
    /// <param name="utf8Xsd">The UTF-8 encoded bytes representing the XSD schema</param>
    /// <exception cref="ArgumentNullException"><paramref name="targetNamespace"/> is <see langword="null"/></exception>
    /// <exception cref="XmlException">The bytes do not represent well-formed XML, or the span is empty</exception>
    /// <exception cref="XmlSchemaException">The bytes contain schema structural or syntax errors</exception>
    void RegisterSchema(string targetNamespace, ReadOnlySpan<byte> utf8Xsd);

    /// <summary>
    /// Determines whether a schema for the specified target namespace is registered in the cache.
    /// </summary>
    /// <param name="targetNamespace">The target XML namespace to look up</param>
    /// <returns><see langword="true"/> if a schema for the specified namespace is registered; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="targetNamespace"/> is <see langword="null"/></exception>
    bool ContainsSchema(string targetNamespace);

    /// <summary>
    /// Gets the total number of precompiled schema sets currently registered in the cache.
    /// </summary>
    /// <value>The total number of precompiled <see cref="XmlSchemaSet"/> instances currently registered.</value>
    int Count { get; }

    /// <summary>
    /// Determines whether the specified root element qualified name is declared as a global element in the registered schema.
    /// </summary>
    /// <param name="targetNamespace">The target XML namespace of the schema</param>
    /// <param name="localName">The local name of the root element</param>
    /// <param name="namespaceUri">The namespace URI of the root element</param>
    /// <returns><see langword="true"/> if the element is declared as a global element in the target schema; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="targetNamespace"/>, <paramref name="localName"/>, or <paramref name="namespaceUri"/> is <see langword="null"/></exception>
    bool IsRootElementDeclared(string targetNamespace, string localName, string namespaceUri);

    /// <summary>
    /// Removes all registered schemas from the cache.
    /// </summary>
    /// <remarks>This operation is thread-safe. Concurrent registrations or lookups in progress will complete normally; subsequent operations will see an empty cache.</remarks>
    void Clear();
}
