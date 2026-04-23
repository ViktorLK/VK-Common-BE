using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core.DependencyInjection.Internal;

namespace VK.Blocks.Core;

/// <summary>
/// Core extension methods for setting up building block services in an <see cref="IServiceCollection"/>.
/// These methods handle the registration and configuration of blocks and their options.
/// </summary>
public static class VKBlockRegistrationExtensions
{
    /// <summary>
    /// Registers a marker in the service collection to indicate that a building block has been initialized.
    /// Following Rule 13 (Mark-Self).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Architecture: Multi-Layered Identity & Validation</b><br/>
    /// This method implements a three-tier safety pattern to ensure building block integrity:
    /// <list type="number">
    /// <item><b>Recursive Validation:</b> Ensures that all prerequisite blocks are already registered.</item>
    /// <item><b>Logical Identity:</b> Prevents different classes from claiming the same logical Identifier (e.g., "Authentication").</item>
    /// <item><b>Typed Contract:</b> Provides zero-reflection, type-safe access to block metadata via DI.</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <typeparam name="TMarker">The marker type representing the building block.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="VKDependencyException">Thrown if a required dependency is missing.</exception>
    public static IServiceCollection AddVKBlockMarker<TMarker>(
        this IServiceCollection services)
        where TMarker : class, IVKBlockMarker, IVKBlockMarkerProvider<TMarker>
    {
        VKGuard.NotNull(services);

        // 1. [Rule 13: Check-Prerequisite] — Idempotency & Recursive Validation
        // This is the "Fail-Fast" gate. Calling the generic IsVKBlockRegistered<TMarker> ensures:
        //   A) Idempotency: If the block (by ID) is already there, we return early.
        //   B) Safety: It recursively walks the dependency tree (IVKBlockMarker.Dependencies)
        //      and throws VKDependencyException if any parent block is missing.
        if (services.IsVKBlockRegistered<TMarker>())
        {
            return services;
        }

        // 2. [Rule 13: Mark-Self] — Logical Identity Registration
        // We register a string-based identifier marker. This protects the system against
        // "Logical Collisions" where two different classes might try to use the same ID.
        // This marker is internal and used by the infrastructure for untyped dependency checks.
        services.AddSingleton(new BlockRuntimeMarker(TMarker.Instance.Identifier));

        // 3. [Identity Registration] — Concrete Type Access
        // We register the concrete TMarker singleton instance.
        // RATIONALE: This allows developers to inject the specific block class (e.g., VKAuthenticationBlock)
        // to access metadata (Version, ActivitySourceName) with ZERO reflection and full type safety.
        services.TryAddSingleton<TMarker>((TMarker)TMarker.Instance);

        return services;
    }

    /// <summary>
    /// Ensures that a required building block is registered before the current block.
    /// Following Rule 13 (Check-Prerequisite).
    /// </summary>
    /// <typeparam name="TRequired">The marker type of the required block.</typeparam>
    /// <typeparam name="TDependent">The marker type of the dependent block.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <exception cref="VKDependencyException">Thrown if the required block is not registered.</exception>
    public static void EnsureVKBlockRegistered<TRequired, TDependent>(this IServiceCollection services)
        where TRequired : class, IVKBlockMarker, IVKBlockMarkerProvider<TRequired>
        where TDependent : class, IVKBlockMarker, IVKBlockMarkerProvider<TDependent>
    {
        if (VKGuard.NotNull(services).IsVKBlockRegistered(TRequired.Instance.Identifier))
        {
            return;
        }

        throw VKDependencyException.MissingDependency(TRequired.Instance.Identifier, TDependent.Instance.Identifier);
    }

    /// <summary>
    /// Shorthand to ensure that the VK.Blocks.Core module is registered.
    /// </summary>
    /// <typeparam name="TBlock">The marker type of the dependent building block.</typeparam>
    /// <param name="services">The service collection.</param>
    public static void EnsureCoreBlockRegistered<TBlock>(this IServiceCollection services)
        where TBlock : class, IVKBlockMarker, IVKBlockMarkerProvider<TBlock>
        => VKGuard.NotNull(services).EnsureVKBlockRegistered<VKCoreBlock, TBlock>();

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
        => VKGuard.NotNull(services).AddVKBlockOptions<TOptions>(VKGuard.NotNull(configuration).GetSection(TOptions.SectionName));

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
    /// </remarks>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section to bind from.</param>
    /// <returns>The eagerly-binded options instance.</returns>
    /// <exception cref="VKDependencyException">Thrown if the options type is already registered but no instance is available.</exception>
    internal static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfigurationSection section)
        where TOptions : class, IVKBlockOptions, new()
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(section);

        // [IDEMPOTENCY CHECK] — Avoid unnecessary Bind and DI registration when already registered
        if (services.IsVKServiceRegistered<TOptions>())
        {
            // Retrieve the already registered singleton instance to avoid re-binding
            return services.GetVKServiceInstance<TOptions>()
                   ?? throw VKDependencyException.DualRegistrationMissing(typeof(TOptions).Name);
        }

        var options = new TOptions();
        section.Bind(options);

        // 1. Singleton registration for direct injection & library-internal synchronous access
        // Registering this early ensures that IsVKServiceRegistered returns true for subsequent calls.
        services.TryAddSingleton(options);

        // 2. Standard Options registration + Validation infrastructure
        services.AddOptions<TOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

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
    /// <exception cref="VKDependencyException">Thrown if the options type is already registered but no instance is available.</exception>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        Action<TOptions>? configure = null)
        where TOptions : class, IVKBlockOptions, new()
    {
        VKGuard.NotNull(services);

        // [IDEMPOTENCY CHECK] — Avoid unnecessary configure and DI registration when already registered
        if (services.IsVKServiceRegistered<TOptions>())
        {
            // Retrieve the already registered singleton instance
            return services.GetVKServiceInstance<TOptions>()
                   ?? throw VKDependencyException.DualRegistrationMissing(typeof(TOptions).Name);
        }

        // To support synchronous return, we MUST create and configure the instance now.
        var options = new TOptions();
        configure?.Invoke(options);

        // 1. Singleton registration (the instance we just configured manually)
        // Registering this early ensures that IsVKServiceRegistered returns true for subsequent calls.
        services.TryAddSingleton(options);

        // 2. Standard Options registration + Validation infrastructure
        OptionsBuilder<TOptions> builder = services.AddOptions<TOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (configure is not null)
        {
            // [UNIFIED HANDLING] We register the configure delegate in the IOptions pipeline.
            // Note: Since IOptions<T> typically creates a NEW instance, this delegate will run once
            // for that new instance, ensuring consistency between the Singleton and IOptions.Value.
            builder.Configure(configure);
        }

        return options;
    }
}
