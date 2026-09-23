// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Xml.Schema;

namespace EricksonLopez.Xml.Validation.Tests.Common;

/// <summary>
/// A test stub for <see cref="IXmlSchemaCache"/> that simulates a cache returning true
/// but with a null <see cref="XmlSchemaSet"/> output, testing defensive validation handling.
/// </summary>
public sealed class NullSchemaCacheMock : IXmlSchemaCache, IXmlSchemaSetProvider
{
    public int Count => 1;
    public bool ContainsSchema(string targetNamespace) => true;
    public bool IsRootElementDeclared(string targetNamespace, string localName, string namespaceUri) => false;
    public void RegisterSchema(string targetNamespace, string xsdContent) { }
    public void RegisterSchema(string targetNamespace, Stream xsdStream) { }
    public void RegisterSchema(string targetNamespace, ReadOnlySpan<byte> utf8Xsd) { }
    public void Clear() { }

    public bool TryGetSchemaSet(string targetNamespace, out XmlSchemaSet? schemaSet)
    {
        schemaSet = null;
        return true;
    }
}
