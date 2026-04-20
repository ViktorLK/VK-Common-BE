using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core.Contracts;

namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Core extension methods for setting up building block services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class VKBlockRegistrationExtensions
{
    /// <summary>
    /// Ensures that a required building block is registered before the current block.
    /// Following Rule 13 (Check-Prerequisite).
    /// </summary>
    /// <typeparam name="TRequired">The marker type of the required block.</typeparam>
    /// <typeparam name="TDependent">The marker type of the dependent block.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <exception cref="InvalidOperationException">Thrown if the required block is not registered.</exception>
    public static void EnsureVKBlockRegistered<TRequired, TDependent>(this IServiceCollection services)
        where TRequired : class, IVKBlockMarker, IVKBlockMarkerProvider<TRequired>
        where TDependent : class, IVKBlockMarker, IVKBlockMarkerProvider<TDependent>
    {
        if (services.IsVKBlockRegistered<TRequired>())
        {
            return;
        }

        throw new InvalidOperationException(
            string.Format(CoreConstants.MissingBlockDependencyMessage, TRequired.Instance.Identifier, TDependent.Instance.Identifier));
    }

    /// <summary>
    /// Shorthand to ensure that the VK.Blocks.Core module is registered.
    /// </summary>
    /// <typeparam name="TBlock">The marker type of the dependent building block.</typeparam>
    /// <param name="services">The service collection.</param>
    public static void EnsureVKCoreBlockRegistered<TBlock>(this IServiceCollection services)
        where TBlock : class, IVKBlockMarker, IVKBlockMarkerProvider<TBlock>
    {
        services.EnsureVKBlockRegistered<CoreBlock, TBlock>();
    }

    /// <summary>
    /// [WRAPPER] Adds and configures a building block's options by automatically resolving the section
    /// name via <see cref="IVKBlockOptions.SectionName"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>PRIORITY 1: Zero-Reflection Resolution</b><br/>
    /// This wrapper leverages C# 11 Static Abstract Members to resolve the section name at compile-time/runtime
    /// without reflection. It internally delegates to the explicit section-based overload.
    /// </para>
    /// <para>
    /// <b>PRIORITY 2: Idempotent Dual-Registration Pattern</b><br/>
    /// Ensures same instance is available via both <c>IOptions&lt;T&gt;</c> and direct <c>Singleton</c>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <returns>The options instance bound at registration time.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TOptions : class, IVKBlockOptions, new()
    {
        return services.AddVKBlockOptions<TOptions>(configuration.GetSection(TOptions.SectionName));
    }

    /// <summary>
    /// [CORE IMPLEMENTATION] Adds and configures options using an explicit configuration section.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>PRIORITY 1: Idempotent Dual-Registration Pattern</b><br/>
    /// Performs a "double registration" of the options while ensuring idempotency:
    /// <list type="number">
    /// <item>Standard <c>IOptions&lt;T&gt;</c> for DI compatibility and lazy-loading.</item>
    /// <item>Direct <c>Singleton</c> for immediate synchronous access within building blocks.</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>PRIORITY 2: Rationale (Why this exists?)</b><br/>
    /// 1. <b>Synchronous Access:</b> Building blocks often need options <i>during</i> the <c>ConfigureServices</c> phase.<br/>
    /// 2. <b>Validation & Performance:</b> Manual <c>Any()</c> checks prevent redundant <c>IStartupValidator</c> registrations.
    /// </para>
    /// <para>
    /// <b>PRIORITY 3: CRITICAL VALIDATION WARNING</b><br/>
    /// This method registers internal validation (Data Annotations).
    /// <b>Custom validators MUST use <c>TryAddEnumerable</c></b>.
    /// Using standard <c>TryAddSingleton</c> will cause custom validators to be skipped.
    /// </para>
    /// </remarks>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section to bind from.</param>
    /// <returns>The eagerly-binded options instance.</returns>
    internal static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfigurationSection section)
        where TOptions : class, IVKBlockOptions, new()
    {
        var options = new TOptions();
        section.Bind(options);

        // [IDEMPOTENCY CHECK]
        if (services.IsVKServiceRegistered<TOptions>())
        {
            return options;
        }

        // Standard Options registration + Validation
        services.AddOptions<TOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Singleton registration for direct injection & library-internal synchronous access
        services.TryAddSingleton(options);

        return options;
    }

    /// <summary>
    /// [DELEGATE VARIANT] Adds and configures options using a manual configuration delegate.
    /// </summary>
    /// <remarks>
    /// Follows the same <c>Idempotent Dual-Registration Pattern</c> as the section-based implementation.
    /// Useful for code-first or unit-testing scenarios.
    /// </remarks>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to configure the options. If null, default values will be used.</param>
    /// <returns>The configured options instance.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        Action<TOptions>? configure = null)
        where TOptions : class, IVKBlockOptions, new()
    {
        var options = new TOptions();
        configure?.Invoke(options);

        // [IDEMPOTENCY CHECK]
        if (services.IsVKServiceRegistered<TOptions>())
        {
            return options;
        }

        // Standard Options registration + Validation
        OptionsBuilder<TOptions> builder = services.AddOptions<TOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (configure is not null)
        {
            builder.Configure(configure);
        }

        // Singleton registration
        services.TryAddSingleton(options);

        return options;
    }

    /// <summary>
    /// Adds and configures a specific feature option using the building block's root configuration context.
    /// Relies on the absolute path provided by <see cref="IVKBlockOptions.SectionName"/>.
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> AddFeatureOptions<TMarker, TOptions>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TOptions : class, IVKBlockOptions, new()
    {
        builder.Services.AddVKBlockOptions<TOptions>(builder.Configuration);
        return builder;
    }

    /// <summary>
    /// Overrides a scoped service registration within the building block.
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> WithScoped<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.Replace(ServiceDescriptor.Scoped<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Overrides a singleton service registration within the building block.
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> WithSingleton<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Overrides a transient service registration within the building block.
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> WithTransient<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.Replace(ServiceDescriptor.Transient<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable scoped service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> TryAddEnumerableScoped<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable singleton service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> TryAddEnumerableSingleton<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable transient service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TMarker">The marker type for the building block.</typeparam>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="builder">The building block builder instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static IVKBlockBuilder<TMarker> TryAddEnumerableTransient<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TMarker : class, IVKBlockMarker
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable scoped service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="services">The service collection instance.</param>
    /// <returns>The service collection instance for chaining.</returns>
    public static IServiceCollection TryAddEnumerableScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<TService, TImplementation>());
        return services;
    }

    /// <summary>
    /// Try to add an enumerable singleton service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="services">The service collection instance.</param>
    /// <returns>The service collection instance for chaining.</returns>
    public static IServiceCollection TryAddEnumerableSingleton<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<TService, TImplementation>());
        return services;
    }

    /// <summary>
    /// Try to add an enumerable transient service registration (idempotent addition).
    /// </summary>
    /// <typeparam name="TService">The service type or interface to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
    /// <param name="services">The service collection instance.</param>
    /// <returns>The service collection instance for chaining.</returns>
    public static IServiceCollection TryAddEnumerableTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<TService, TImplementation>());
        return services;
    }
}
