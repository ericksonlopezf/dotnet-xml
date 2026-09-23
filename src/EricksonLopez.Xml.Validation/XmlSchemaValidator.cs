// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using EricksonLopez.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EricksonLopez.Xml.Validation;

/// <summary>
/// Validates XML documents against precompiled XSD schemas while enforcing Anti-XXE protection and structured validation results.
/// </summary>
/// <remarks>
/// Prohibits inline DTD processing and external entity resolution across all synchronous and asynchronous validation paths.
/// Schema validation errors and warnings are captured without throwing runtime exceptions, returning a strongly-typed
/// <see cref="Result{T}"/> indicating validation success or failure with diagnostic details.
/// </remarks>
public sealed partial class XmlSchemaValidator : IXmlSchemaValidator
{
    private readonly IXmlSchemaCache _schemaCache;
    private readonly XmlValidationOptions _options;
    private readonly ILogger<XmlSchemaValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidator"/> class using default validation options.
    /// </summary>
    /// <param name="schemaCache">The schema cache containing precompiled XSD schema sets</param>
    /// <remarks>
    /// The supplied <paramref name="schemaCache"/> must implement the library's internal schema set provider (such as <see cref="XmlSchemaCache"/>).
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="schemaCache"/> is <see langword="null"/></exception>
    public XmlSchemaValidator(IXmlSchemaCache schemaCache)
        : this(schemaCache, new XmlValidationOptions(), NullLogger<XmlSchemaValidator>.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlSchemaValidator"/> class with custom validation options and diagnostic logging.
    /// </summary>
    /// <param name="schemaCache">The schema cache containing precompiled XSD schema sets</param>
    /// <param name="options">The configuration options controlling warning inclusion and severity</param>
    /// <param name="logger">The logger instance used for structured diagnostic logging.
    /// When <see langword="null"/>, a <see cref="Microsoft.Extensions.Logging.Abstractions.NullLogger{T}"/> is substituted and no diagnostic output is emitted.</param>
    /// <remarks>
    /// The supplied <paramref name="schemaCache"/> must implement the library's internal schema set provider (such as <see cref="XmlSchemaCache"/>).
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="schemaCache"/> or <paramref name="options"/> is <see langword="null"/></exception>
    public XmlSchemaValidator(
        IXmlSchemaCache schemaCache,
        XmlValidationOptions options,
        ILogger<XmlSchemaValidator>? logger = null)
    {
        _schemaCache = schemaCache ?? throw new ArgumentNullException(nameof(schemaCache));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<XmlSchemaValidator>.Instance;
    }

    /// <inheritdoc/>
    public Result<bool> Validate(string xml, string targetNamespace)
    {
        ArgumentNullException.ThrowIfNull(xml);
        ArgumentNullException.ThrowIfNull(targetNamespace);

        using var reader = new StringReader(xml);
        return ValidateInternal(reader, targetNamespace);
    }

    /// <inheritdoc/>
    public Result<bool> Validate(Stream xmlStream, string targetNamespace)
    {
        ArgumentNullException.ThrowIfNull(xmlStream);
        ArgumentNullException.ThrowIfNull(targetNamespace);

        return ValidateStreamInternal(xmlStream, targetNamespace);
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Vulnerability", "S6640:Avoid using unsafe code blocks", Justification = "UnmanagedMemoryStream over ReadOnlySpan is required for zero-allocation schema validation.")]
    public unsafe Result<bool> Validate(ReadOnlySpan<byte> utf8Xml, string targetNamespace)
    {
        ArgumentNullException.ThrowIfNull(targetNamespace);

        if (utf8Xml.IsEmpty)
        {
            var message = "Malformed XML: XML document payload is empty.";
            LogMalformedXml(_logger, targetNamespace, 0, 0, message);
            return Error.Validation("XmlValidation.XmlMalformed", message);
        }

        fixed (byte* ptr = utf8Xml)
        {
            using var memoryStream = new UnmanagedMemoryStream(ptr, utf8Xml.Length, utf8Xml.Length, FileAccess.Read);
            return ValidateStreamInternal(memoryStream, targetNamespace);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> ValidateAsync(
        Stream xmlStream,
        string targetNamespace,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(xmlStream);
        ArgumentNullException.ThrowIfNull(targetNamespace);

        cancellationToken.ThrowIfCancellationRequested();

        if (!TryGetSchemaSet(targetNamespace, out var schemaSet) || schemaSet is null)
        {
            LogSchemaNotRegistered(_logger, targetNamespace);

            return Error.NotFound(
                "XmlValidation.SchemaNotRegistered",
                $"No precompiled XSD schema registered for namespace '{targetNamespace}'.");
        }

        var errors = new List<string>();
        var warnings = new List<string>();
        var settings = CreateValidationSettings(schemaSet, errors, warnings);

        try
        {
            using var xmlReader = XmlReader.Create(xmlStream, settings);
            var rootValidated = false;
            while (await xmlReader.ReadAsync().ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!rootValidated && xmlReader.NodeType == XmlNodeType.Element)
                {
                    rootValidated = true;
                    ValidateRootElement(xmlReader, targetNamespace, errors);
                }
            }

            return BuildResult(errors, warnings, targetNamespace);
        }
        catch (OperationCanceledException)
        {
            LogValidationCancelled(_logger, targetNamespace);
            throw;
        }
        catch (XmlException ex)
        {
            var message = $"Malformed XML at Line {ex.LineNumber}, Position {ex.LinePosition}: {ex.Message}";
            LogMalformedXml(_logger, targetNamespace, ex.LineNumber, ex.LinePosition, ex.Message);

            return Error.Validation("XmlValidation.XmlMalformed", message);
        }
    }

    private Result<bool> ValidateInternal(TextReader textReader, string targetNamespace)
    {
        if (!TryGetSchemaSet(targetNamespace, out var schemaSet) || schemaSet is null)
        {
            LogSchemaNotRegistered(_logger, targetNamespace);

            return Error.NotFound(
                "XmlValidation.SchemaNotRegistered",
                $"No precompiled XSD schema registered for namespace '{targetNamespace}'.");
        }

        var errors = new List<string>();
        var warnings = new List<string>();
        var settings = CreateValidationSettings(schemaSet, errors, warnings);

        try
        {
            using var xmlReader = XmlReader.Create(textReader, settings);
            return ReadAndValidate(xmlReader, errors, warnings, targetNamespace);
        }
        catch (XmlException ex)
        {
            var message = $"Malformed XML at Line {ex.LineNumber}, Position {ex.LinePosition}: {ex.Message}";
            LogMalformedXml(_logger, targetNamespace, ex.LineNumber, ex.LinePosition, ex.Message);

            return Error.Validation("XmlValidation.XmlMalformed", message);
        }
    }

    private Result<bool> ValidateStreamInternal(Stream stream, string targetNamespace)
    {
        if (!TryGetSchemaSet(targetNamespace, out var schemaSet) || schemaSet is null)
        {
            LogSchemaNotRegistered(_logger, targetNamespace);

            return Error.NotFound(
                "XmlValidation.SchemaNotRegistered",
                $"No precompiled XSD schema registered for namespace '{targetNamespace}'.");
        }

        var errors = new List<string>();
        var warnings = new List<string>();
        var settings = CreateValidationSettings(schemaSet, errors, warnings);

        try
        {
            using var xmlReader = XmlReader.Create(stream, settings);
            return ReadAndValidate(xmlReader, errors, warnings, targetNamespace);
        }
        catch (XmlException ex)
        {
            var message = $"Malformed XML at Line {ex.LineNumber}, Position {ex.LinePosition}: {ex.Message}";
            LogMalformedXml(_logger, targetNamespace, ex.LineNumber, ex.LinePosition, ex.Message);

            return Error.Validation("XmlValidation.XmlMalformed", message);
        }
    }

    private Result<bool> ReadAndValidate(
        XmlReader xmlReader,
        List<string> errors,
        List<string> warnings,
        string targetNamespace)
    {
        var rootValidated = false;
        while (xmlReader.Read())
        {
            if (!rootValidated && xmlReader.NodeType == XmlNodeType.Element)
            {
                rootValidated = true;
                ValidateRootElement(xmlReader, targetNamespace, errors);
            }
        }

        return BuildResult(errors, warnings, targetNamespace);
    }

    private void ValidateRootElement(
        XmlReader xmlReader,
        string targetNamespace,
        List<string> errors)
    {
        if (!_schemaCache.IsRootElementDeclared(targetNamespace, xmlReader.LocalName, xmlReader.NamespaceURI))
        {
            var lineInfo = (IXmlLineInfo)xmlReader;
            errors.Add($"Line {lineInfo.LineNumber}, Pos {lineInfo.LinePosition}: Root element '{xmlReader.LocalName}' in namespace '{xmlReader.NamespaceURI}' is not declared in the target schema.");
        }
    }

    private Result<bool> BuildResult(List<string> errors, List<string> warnings, string targetNamespace)
    {
        var hasErrors = errors.Count > 0;
        var hasWarnings = _options.TreatWarningsAsErrors && warnings.Count > 0;

        if (hasErrors || hasWarnings)
        {
            var allMessages = (hasErrors, _options.IncludeWarnings) switch
            {
                (true, true) => errors.Concat(warnings),
                (true, false) => errors,
                _ => warnings
            };

            var errorMessage = string.Join(" | ", allMessages);
            LogValidationFailed(_logger, targetNamespace, errorMessage);
            return Error.Validation("XmlValidation.SchemaViolation", errorMessage);
        }

        LogValidationSucceeded(_logger, targetNamespace);
        return Result<bool>.Success(true);
    }

    private bool TryGetSchemaSet(string targetNamespace, out XmlSchemaSet? schemaSet)
    {
        if (_schemaCache is IXmlSchemaSetProvider provider)
        {
            return provider.TryGetSchemaSet(targetNamespace, out schemaSet);
        }

        throw new InvalidOperationException($"The provided IXmlSchemaCache instance ({_schemaCache.GetType().Name}) must implement the internal IXmlSchemaSetProvider interface to be used by the validator.");
    }

    private XmlReaderSettings CreateValidationSettings(
        XmlSchemaSet schemaSet,
        List<string> errors,
        List<string> warnings)
    {
        // Enforce anti-XXE defense: neutralize any resolver on the schemaSet in case it was mutated externally
        schemaSet.XmlResolver = null;

        var flags = XmlSchemaValidationFlags.ProcessSchemaLocation |
                    XmlSchemaValidationFlags.ReportValidationWarnings |
                    XmlSchemaValidationFlags.ProcessIdentityConstraints |
                    (_options.ProcessInlineSchema ? XmlSchemaValidationFlags.ProcessInlineSchema : XmlSchemaValidationFlags.None);

        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemaSet,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            CloseInput = false,
            // Async = true is intentionally set on all paths (sync and async alike).
            // A single settings factory is shared across both ValidateInternal (sync)
            // and ValidateAsync (async) so that both code paths benefit from the same
            // reader configuration. Synchronous callers of XmlReader.Read() work
            // correctly even when the reader is constructed with Async = true.
            Async = true,
            MaxCharactersInDocument = _options.MaxCharactersInDocument,
            ValidationFlags = flags
        };

        settings.ValidationEventHandler += (_, args) =>
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                if (errors.Count < _options.MaxErrors)
                {
                    errors.Add($"Line {args.Exception.LineNumber}, Pos {args.Exception.LinePosition}: {args.Message}");
                }
            }
            else if (_options.IncludeWarnings)
            {
                if (warnings.Count < _options.MaxErrors)
                {
                    warnings.Add($"[Warning] Line {args.Exception.LineNumber}, Pos {args.Exception.LinePosition}: {args.Message}");
                }
            }
        };

        return settings;
    }

    // ─── [LoggerMessage] zero-allocation delegates ─────────────────────────────

    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug,
        Message = "Schema not registered for namespace '{Namespace}'")]
    private static partial void LogSchemaNotRegistered(ILogger logger, string @namespace);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Debug,
        Message = "Validation failed for namespace '{Namespace}': {Errors}")]
    private static partial void LogValidationFailed(ILogger logger, string @namespace, string errors);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Debug,
        Message = "Validation succeeded for namespace '{Namespace}'")]
    private static partial void LogValidationSucceeded(ILogger logger, string @namespace);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Debug,
        Message = "Validation cancelled for namespace '{Namespace}'")]
    private static partial void LogValidationCancelled(ILogger logger, string @namespace);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Debug,
        Message = "Malformed XML at Line {LineNumber}, Position {LinePosition} for namespace '{Namespace}': {XmlMessage}")]
    private static partial void LogMalformedXml(
        ILogger logger, string @namespace, int lineNumber, int linePosition, string xmlMessage);
}
