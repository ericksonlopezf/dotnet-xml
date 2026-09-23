// Copyright © Erickson Lopez. MIT License.
using System;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace EricksonLopez.Xml.Validation.Tests.Common;

/// <summary>
/// Diagnostic reflection helpers for testing internal invariants of System.Xml types.
/// </summary>
internal static class XmlSchemaTestHelper
{
    private static readonly FieldInfo? ReaderSettingsField =
        typeof(XmlSchemaSet).GetField("_readerSettings", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? typeof(XmlSchemaSet).GetField("readerSettings", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? ResolverField =
        typeof(XmlReaderSettings).GetField("_xmlResolver", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? typeof(XmlReaderSettings).GetField("xmlResolver", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>
    /// Reads the internal XmlResolver of an XmlSchemaSet (stored in its internal _readerSettings)
    /// to verify Anti-XXE invariants.
    /// In standard BCL XmlSchemaSet, XmlResolver only exposes a 'set' accessor, necessitating field inspection.
    /// </summary>
    public static XmlResolver? GetXmlResolver(this XmlSchemaSet schemaSet)
    {
        ArgumentNullException.ThrowIfNull(schemaSet);
        if (ReaderSettingsField is null || ResolverField is null)
        {
            throw new InvalidOperationException("Failed to locate _readerSettings or _xmlResolver fields via reflection.");
        }

        var readerSettings = ReaderSettingsField.GetValue(schemaSet);
        if (readerSettings is null)
        {
            throw new InvalidOperationException("Internal _readerSettings instance was null on XmlSchemaSet.");
        }

        return ResolverField.GetValue(readerSettings) as XmlResolver;
    }
}
