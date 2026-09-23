// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Xml.Validation;

/// <summary>
/// Specifies configuration options for XML schema validation behavior.
/// </summary>
/// <remarks>
/// Instances are configured when registering XML validation services via
/// <see cref="XmlValidationServiceCollectionExtensions.AddXmlValidation(Microsoft.Extensions.DependencyInjection.IServiceCollection, Action{XmlValidationOptions})"/>.
/// </remarks>
public sealed class XmlValidationOptions
{
    private bool _includeWarnings;
    private bool _treatWarningsAsErrors;

    /// <summary>
    /// Gets or sets a value indicating whether non-fatal XSD schema warnings are included in validation results.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if warnings are included in validation results; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// When <see langword="false"/> (default), only schema errors are reported unless <see cref="TreatWarningsAsErrors"/> is enabled.
    /// When <see langword="true"/>, warnings are captured and appended with the prefix <c>[Warning]</c>.
    /// If <see cref="TreatWarningsAsErrors"/> is enabled, this property always returns <see langword="true"/> to prevent silent warning drops.
    /// </remarks>
    public bool IncludeWarnings
    {
        get => _includeWarnings || _treatWarningsAsErrors;
        set => _includeWarnings = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether XSD schema warnings are treated as validation errors.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if schema warnings cause validation to fail; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Setting this to <see langword="true"/> ensures schema warnings cause validation failure, and automatically includes warnings in the result.
    /// </remarks>
    public bool TreatWarningsAsErrors
    {
        get => _treatWarningsAsErrors;
        set => _treatWarningsAsErrors = value;
    }

    private long _maxCharactersInDocument = 10_000_000;
    private int _maxErrors = 100;

    /// <summary>
    /// Gets or sets the maximum number of characters allowed in the XML document to defend against denial of service.
    /// </summary>
    /// <value>
    /// The maximum allowed characters, or <c>0</c> if unlimited. The default is 10,000,000.
    /// </value>
    /// <remarks>
    /// Set to <c>0</c> for unlimited character count. Values must be greater than or equal to zero.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than zero</exception>
    public long MaxCharactersInDocument
    {
        get => _maxCharactersInDocument;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "MaxCharactersInDocument must be greater than or equal to zero.");
            }
            _maxCharactersInDocument = value;
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of validation errors and warnings collected before truncating further additions.
    /// </summary>
    /// <value>
    /// The maximum number of validation errors and warnings to collect. The default is 100.
    /// </value>
    /// <remarks>
    /// Enforces an upper limit on collected diagnostic messages to defend against memory exhaustion on malformed inputs.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than or equal to zero</exception>
    public int MaxErrors
    {
        get => _maxErrors;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "MaxErrors must be greater than zero.");
            }
            _maxErrors = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether inline schemas encountered in the XML document are processed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if inline schemas are processed; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The default is <see langword="false"/> to defend against inline schema poisoning on untrusted input.
    /// Enable only when processing documents with trusted inline schemas.
    /// </remarks>
    public bool ProcessInlineSchema { get; set; }
}
