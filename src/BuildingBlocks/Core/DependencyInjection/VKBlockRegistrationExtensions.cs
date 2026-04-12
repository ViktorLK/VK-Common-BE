using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Core extension methods for setting up building block services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class VKBlockRegistrationExtensions
{
    /// <summary>
    /// Adds and configures a building block's options using a specific configuration section.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>ARCHITECTURE NOTE (Idempotent Dual-Registration Pattern):</b><br/>
    /// This method performs a "double registration" of the options while ensuring idempotency:
    /// <list type="number">
    /// <item>Standard <c>IOptions&lt;T&gt;</c> registration for lazy-loading and ASP.NET Core compatibility.</item>
    /// <item>Direct <c>Singleton</c> registration of the same instance for eager-loading.</item>
    /// </list>
    /// <b>Why?</b><br/>
    /// 1. <b>Library Internal Access:</b> Building blocks often need synchronous access to their options during the
    /// <c>ConfigureServices</c> phase.<br/>
    /// 2. <b>Validation & Performance:</b> Manual <c>services.Any()</c> checks prevent redundant <c>IStartupValidator</c>
    /// registrations, while <c>TryAddSingleton</c> ensures container level safety.
    /// <b>Validation Warning:</b><br/>
    /// Because this method internally registers <c>IValidateOptions&lt;TOptions&gt;</c> (via <c>ValidateDataAnnotations</c> and <c>ValidateOnStart</c>), 
    /// any subsequent registration of custom validators MUST use <c>TryAddEnumerable</c> or <c>TryAddEnumerableSingleton</c>. 
    /// Using <c>TryAddSingleton</c> for a custom validator will result in the validator being skipped because the options system 
    /// has already registered the built-in validation services.
    /// </para>
    /// </remarks>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section to bind from.</param>
    /// <returns>The eagerly-binded options instance for immediate use in the registration pipeline.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration section)
        where TOptions : class, new()
    {
        var options = new TOptions();
        section.Bind(options);

        // [IDEMPOTENCY CHECK]
        // If the options type is already registered in the container, it means
        // another module has already configured the options system and validation.
        // We skip re-registration to avoid duplicate IStartupValidator instances.
        if (services.Any(d => d.ServiceType == typeof(TOptions)))
        {
            return options;
        }

        // Standard Options registration. 
        // NOTE: These internal validation calls register IValidateOptions<TOptions>.
        // Custom validators MUST use TryAddEnumerable to avoid being blocked by these registrations.
        services.AddOptions<TOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Singleton registration for direct injection & library-internal synchronous access
        services.TryAddSingleton(options);

        return options;
    }

    /// <summary>
    /// Adds and configures a building block's options using a manual configuration delegate.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>ARCHITECTURE NOTE (Idempotent Dual-Registration Pattern):</b><br/>
    /// This method performs a "double registration" of the options while ensuring idempotency:
    /// <list type="number">
    /// <item>Standard <c>IOptions&lt;T&gt;</c> registration for lazy-loading and ASP.NET Core compatibility.</item>
    /// <item>Direct <c>Singleton</c> registration of the same instance for eager-loading.</item>
    /// </list>
    /// <b>Why?</b><br/>
    /// 1. <b>Library Internal Access:</b> Building blocks often need synchronous access to their options during the
    /// <c>ConfigureServices</c> phase.<br/>
    /// 2. <b>Validation & Performance:</b> Manual <c>services.Any()</c> checks prevent redundant <c>IStartupValidator</c>
    /// registrations, while <c>TryAddSingleton</c> ensures container level safety.
    /// </para>
    /// <para>
    /// <b>Validation Warning:</b><br/>
    /// Because this method internally registers <c>IValidateOptions&lt;TOptions&gt;</c> (via <c>ValidateDataAnnotations</c> and <c>ValidateOnStart</c>), 
    /// any subsequent registration of custom validators MUST use <c>TryAddEnumerable</c> or <c>TryAddEnumerableSingleton</c>. 
    /// Using <c>TryAddSingleton</c> for a custom validator will result in the validator being skipped because the options system 
    /// has already registered the built-in validation services.
    /// </para>
    /// </remarks>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Delegate to configure the options.</param>
    /// <returns>The configured options instance.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        Action<TOptions> configure)
        where TOptions : class, new()
    {
        var options = new TOptions();
        configure(options);

        // [IDEMPOTENCY CHECK]
        if (services.Any(d => d.ServiceType == typeof(TOptions)))
        {
            return options;
        }

        // Standard Options registration.
        // NOTE: These internal validation calls register IValidateOptions<TOptions>.
        // Custom validators MUST use TryAddEnumerable to avoid being blocked by these registrations.
        services.AddOptions<TOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Singleton registration
        services.TryAddSingleton(options);

        return options;
    }

    /// <summary>
    /// Overrides a scoped service registration within the building block.
    /// </summary>
    /// <typeparam name="TMarker">The block marker.</typeparam>
    /// <typeparam name="TService">The service interface/base type.</typeparam>
    /// <typeparam name="TImplementation">The custom implementation type.</typeparam>
    public static IVKBlockBuilder<TMarker> WithScoped<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.Replace(ServiceDescriptor.Scoped<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Overrides a singleton service registration within the building block.
    /// </summary>
    /// <typeparam name="TMarker">The block marker.</typeparam>
    /// <typeparam name="TService">The service interface/base type.</typeparam>
    /// <typeparam name="TImplementation">The custom implementation type.</typeparam>
    public static IVKBlockBuilder<TMarker> WithSingleton<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Overrides a transient service registration within the building block.
    /// </summary>
    /// <typeparam name="TMarker">The block marker.</typeparam>
    /// <typeparam name="TService">The service interface/base type.</typeparam>
    /// <typeparam name="TImplementation">The custom implementation type.</typeparam>
    public static IVKBlockBuilder<TMarker> WithTransient<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.Replace(ServiceDescriptor.Transient<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable scoped service registration (idempotent addition).
    /// </summary>
    public static IVKBlockBuilder<TMarker> TryAddEnumerableScoped<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable singleton service registration (idempotent addition).
    /// </summary>
    public static IVKBlockBuilder<TMarker> TryAddEnumerableSingleton<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<TService, TImplementation>());
        return builder;
    }

    /// <summary>
    /// Try to add an enumerable transient service registration (idempotent addition).
    /// </summary>
    public static IVKBlockBuilder<TMarker> TryAddEnumerableTransient<TMarker, TService, TImplementation>(
        this IVKBlockBuilder<TMarker> builder)
        where TService : class
        where TImplementation : class, TService
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<TService, TImplementation>());
        return builder;
    }



    #region IServiceCollection Extensions

    /// <summary>
    /// Try to add an enumerable scoped service registration (idempotent addition).
    /// </summary>
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
    public static IServiceCollection TryAddEnumerableTransient<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<TService, TImplementation>());
        return services;
    }

    #endregion
}
