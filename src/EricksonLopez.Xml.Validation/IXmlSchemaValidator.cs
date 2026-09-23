// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;

namespace EricksonLopez.Xml.Validation;

/// <summary>
/// Defines a contract for validating XML documents against precompiled XSD schemas with Anti-XXE enforcement.
/// </summary>
/// <remarks>
/// Implementations must enforce strict Anti-XXE security by prohibiting DTD processing and external entity resolution.
/// Validation results are returned as strongly-typed <see cref="Result{T}"/> instances rather than throwing exceptions for validation failures.
/// <para>
/// When used with the standard <see cref="XmlSchemaValidator"/> implementation, the supplied <see cref="IXmlSchemaCache"/>
/// must be backed by a provider that exposes precompiled schema sets (such as <see cref="XmlSchemaCache"/>).
/// </para>
/// </remarks>
public interface IXmlSchemaValidator
{
    /// <summary>
    /// Validates an XML string against the precompiled schema associated with the specified namespace.
    /// </summary>
    /// <param name="xml">The XML document string to validate</param>
    /// <param name="targetNamespace">The target XML namespace of the schema to validate against</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing <see langword="true"/> if the XML is valid;
    /// otherwise, a failure result containing structured error details.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="xml"/> or <paramref name="targetNamespace"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The configured schema cache does not implement the schema set provider required for validation.</exception>
    Result<bool> Validate(string xml, string targetNamespace);

    /// <summary>
    /// Validates an XML stream against the precompiled schema associated with the specified namespace.
    /// </summary>
    /// <param name="xmlStream">The readable stream containing the XML document data</param>
    /// <param name="targetNamespace">The target XML namespace of the schema to validate against</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing <see langword="true"/> if the XML is valid;
    /// otherwise, a failure result containing structured error details.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="xmlStream"/> or <paramref name="targetNamespace"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The configured schema cache does not implement the schema set provider required for validation.</exception>
    Result<bool> Validate(Stream xmlStream, string targetNamespace);

    /// <summary>
    /// Validates a UTF-8 encoded XML byte sequence against the precompiled schema associated with the specified namespace.
    /// </summary>
    /// <remarks>
    /// For high-throughput scenarios, prefer reusing streams where feasible, or use string buffers if the payload is already transcoded.
    /// While validating from a <see cref="ReadOnlySpan{T}"/> prevents payload array allocation via unmanaged memory pinning,
    /// the underlying XML reader implementation will still allocate internal decoding buffers on the Gen 0 heap.
    /// </remarks>
    /// <param name="utf8Xml">The UTF-8 encoded byte sequence representing the XML document</param>
    /// <param name="targetNamespace">The target XML namespace of the schema to validate against</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing <see langword="true"/> if the XML is valid;
    /// otherwise, a failure result containing structured error details.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="targetNamespace"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The configured schema cache does not implement the schema set provider required for validation.</exception>
    Result<bool> Validate(ReadOnlySpan<byte> utf8Xml, string targetNamespace);

    /// <summary>
    /// Validates an XML stream asynchronously against the precompiled schema associated with the specified namespace.
    /// </summary>
    /// <param name="xmlStream">The readable stream containing the XML document data</param>
    /// <param name="targetNamespace">The target XML namespace of the schema to validate against</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a successful
    /// <see cref="Result{T}"/> with <see langword="true"/> if the XML is valid; otherwise, a failure
    /// result containing structured error details.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="xmlStream"/> or <paramref name="targetNamespace"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via <paramref name="cancellationToken"/></exception>
    /// <exception cref="InvalidOperationException">The configured schema cache does not implement the schema set provider required for validation.</exception>
    Task<Result<bool>> ValidateAsync(Stream xmlStream, string targetNamespace, CancellationToken cancellationToken = default);
}
