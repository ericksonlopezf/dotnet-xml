// Copyright © Erickson Lopez. MIT License.
using System.Xml.Schema;
using EricksonLopez.Xml.Validation;

namespace EricksonLopez.Xml.Validation.Tests.Common;

internal static class XmlSchemaCacheTestExtensions
{
    public static bool TryGetSchemaSet(this IXmlSchemaCache cache, string targetNamespace, out XmlSchemaSet? schemaSet)
    {
        if (cache is IXmlSchemaSetProvider provider)
        {
            return provider.TryGetSchemaSet(targetNamespace, out schemaSet);
        }

        schemaSet = null;
        return false;
    }

    public static bool TryGetSchemaSet(this XmlSchemaCache cache, string targetNamespace, out XmlSchemaSet? schemaSet)
    {
        if (cache is IXmlSchemaSetProvider provider)
        {
            return provider.TryGetSchemaSet(targetNamespace, out schemaSet);
        }

        schemaSet = null;
        return false;
    }
}
