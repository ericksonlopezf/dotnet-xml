// Copyright © Erickson Lopez. MIT License.
using System.Xml.Schema;

namespace EricksonLopez.Xml.Validation;

/// <summary>
/// Provides internal access to the compiled <see cref="XmlSchemaSet"/> from the cache.
/// </summary>
/// <remarks>
/// This interface is intentionally internal to prevent consumers from mutating the global cache,
/// thus securing the library against thread-safety bypasses (XML-API-001).
/// </remarks>
internal interface IXmlSchemaSetProvider
{
    /// <summary>
    /// Attempts to retrieve the precompiled <see cref="XmlSchemaSet"/> for the specified namespace.
    /// </summary>
    /// <param name="targetNamespace">The target XML namespace of the schema to look up</param>
    /// <param name="schemaSet">
    /// When this method returns, contains the precompiled <see cref="XmlSchemaSet"/> associated with the target namespace if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the schema set was found in the cache; otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetSchemaSet(string targetNamespace, out XmlSchemaSet? schemaSet);
}
