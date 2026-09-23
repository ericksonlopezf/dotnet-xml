// Copyright © Erickson Lopez. MIT License.
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EricksonLopez.Xml.Validation;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/>
/// to register XML schema validation services and caching components.
/// </summary>
public static class XmlValidationServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IXmlSchemaCache"/> and <see cref="IXmlSchemaValidator"/> as singletons
    /// using default <see cref="XmlValidationOptions"/>.
    /// </summary>
    /// <param name="services">The service collection to register services into</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddXmlValidation(this IServiceCollection services)
        => AddXmlValidation(services, configure: null);

    /// <summary>
    /// Registers <see cref="IXmlSchemaCache"/> and <see cref="IXmlSchemaValidator"/> as singletons
    /// with configurable <see cref="XmlValidationOptions"/>.
    /// </summary>
    /// <param name="services">The service collection to register services into</param>
    /// <param name="configure">
    /// An optional delegate to configure <see cref="XmlValidationOptions"/>.
    /// When <see langword="null"/>, options configured via <see cref="IOptions{TOptions}"/> or default options are used.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    /// <example>
    /// <code>
    /// services.AddXmlValidation(options =>
    /// {
    ///     options.IncludeWarnings = true;
    ///     options.TreatWarningsAsErrors = false;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddXmlValidation(
        this IServiceCollection services,
        Action<XmlValidationOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new XmlValidationOptions();
        configure?.Invoke(options);

        services.TryAddSingleton<IXmlSchemaCache, XmlSchemaCache>();
        services.TryAddSingleton<IXmlSchemaValidator>(sp =>
        {
            var cache = sp.GetRequiredService<IXmlSchemaCache>();
            var logger = sp.GetService<ILogger<XmlSchemaValidator>>();
            var effectiveOptions = configure is not null
                ? options
                : sp.GetService<IOptions<XmlValidationOptions>>()?.Value ?? options;
            return new XmlSchemaValidator(cache, effectiveOptions, logger);
        });

        return services;
    }
}
