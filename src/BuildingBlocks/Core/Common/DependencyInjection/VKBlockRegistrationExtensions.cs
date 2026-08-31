using System;
using System.Linq;
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
    /// Following AP.02 (Mark-Self).
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

        // 1. [AP.02: Check-Prerequisite] 窶・Idempotency & Recursive Validation
        // This is the "Fail-Fast" gate. Calling the generic IsVKBlockRegistered<TMarker> ensures:
        //   A) Idempotency: If the block (by ID) is already there, we return early.
        //   B) Safety: It recursively walks the dependency tree (IVKBlockMarker.Dependencies)
        //      and throws VKDependencyException if any parent block is missing.
        if (services.IsVKBlockRegistered<TMarker>())
        {
            return services;
        }

        // 2. [AP.02: Mark-Self] 窶・Logical Identity Registration
        // We register a string-based identifier marker. This protects the system against
        // "Logical Collisions" where two different classes might try to use the same ID.
        // This marker is internal and used by the infrastructure for untyped dependency checks.
        services.AddSingleton(new BlockRuntimeMarker(TMarker.Instance.Identifier));

        // 3. [Identity Registration] 窶・Concrete Type Access
        // We register the concrete TMarker singleton instance.
        // RATIONALE: This allows developers to inject the specific block class (e.g., VKAuthenticationBlock)
        // to access metadata (Version, ActivitySourceName) with ZERO reflection and full type safety.
        services.TryAddSingleton<TMarker>((TMarker)TMarker.Instance);

        return services;
    }

    /// <summary>
    /// Ensures that a required building block is registered before the current block.
    /// Following AP.02 (Check-Prerequisite).
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
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>PRIORITY 1: Zero-Reflection Resolution</b><br/>
    /// This wrapper leverages C# 11 Static Abstract Members to resolve the section name at compile-time/runtime
    /// without reflection.
    /// </para>
    /// <para>
    /// <b>PRIORITY 2: Idempotent Dual-Registration Pattern</b><br/>
    /// [AP.04 / BB.05: Immutable Options Registration Pattern]
    /// Binds and registers an <see cref="IVKBlockOptions"/> instance using zero-reflection and immutable records.
    /// Following ADR-016: Supports fluent mutation via 'with' expressions without runtime reflection or post-configuration mutation.
    /// </summary>
    /// <typeparam name="TOptions">The concrete options type implementing <see cref="IVKBlockOptions"/>.</typeparam>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The configuration to bind against (Section: <see cref="IVKBlockOptions.SectionName"/>).</param>
    /// <param name="transform">Optional transformation function to modify the options using 'with' expressions.</param>
    /// <returns>The options instance bound at registration time.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<TOptions, TOptions>? transform = null)
        where TOptions : class, IVKBlockOptions, new()
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);
        VKGuard.Against(string.IsNullOrWhiteSpace(TOptions.SectionName), "Options SectionName cannot be null or empty.");

        // [IDEMPOTENCY CHECK] – Avoid unnecessary Bind and DI registration when already registered
        if (services.IsVKServiceRegistered<TOptions>())
        {
            var existingOptions = services.GetVKServiceInstance<TOptions>()
                   ?? throw VKDependencyException.DualRegistrationMissing(typeof(TOptions).Name);

            // ADR-016: If a subsequent fluent transform is provided, apply it to the existing instance
            // and replace the registered singleton and OptionsFactory in the DI container.
            if (transform is not null)
            {
                var transformedOptions = transform(existingOptions);

                // NOTE (CONCURRENCY & THREAD-SAFETY WARNING):
                // IServiceCollection is not thread-safe and is strictly intended for single-threaded configuration.
                // These two 'Replace' operations are non-atomic. In highly dynamic, multi-threaded registration contexts,
                // there is a temporary race-condition window where direct injection (resolving transformedOptions)
                // and IOptions/IOptionsFactory injection (resolving existingOptions) might become desynchronized.
                // Reconfiguration/fluent transforms MUST only occur during the synchronous single-threaded Startup/Configure phase.
                services.Replace(ServiceDescriptor.Singleton(transformedOptions));
                var registry = GetOrCreateRegistry<TOptions>(services);
                registry.Set(Options.DefaultName, transformedOptions);

                return transformedOptions;
            }


            return existingOptions;
        }

        // A. BINDING OPTIMIZATION
        // Following .NET 10 best practices, use .Get<T>() which is more friendly to records and init properties
        // as it handles the instantiation process via the Binder.
        var targetConfig = configuration.GetSection(TOptions.SectionName);

        var options = targetConfig.Get<TOptions>() ?? new TOptions();

        if (transform is not null)
        {
            // ADR-016: Functional transformation using 'with' expression
            options = transform(options);
        }

        // 1. Registering this early ensures that IsVKServiceRegistered returns true for subsequent calls.
        services.TryAddSingleton(options);

        // 2. Registry + Factory for IOptions pipeline
        var reg = GetOrCreateRegistry<TOptions>(services);
        reg.Set(Options.DefaultName, options);

        // 3. Validation infrastructure (Still needed for startup check)
        services.AddOptions<TOptions>().ValidateDataAnnotations().ValidateOnStart();

        return options;
    }

    /// <summary>
    /// [WRAPPER] Bridge overload for standard <see cref="Action{TOptions}"/> configuration delegate.
    /// Converts the mutating action into the immutable <c>Func&lt;TOptions, TOptions&gt;</c> transformation pipeline per ADR-016.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <param name="configure">Optional standard Action-based configuration delegate.</param>
    /// <returns>The options instance bound at registration time.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TOptions>? configure)
        where TOptions : class, IVKBlockOptions, new()
    {
        Func<TOptions, TOptions>? transform = configure is null
            ? null
            : options =>
            {
                configure(options);
                return options;
            };

        return services.AddVKBlockOptions<TOptions>(
            configuration,
            transform);
    }

    /// <summary>
    /// [WRAPPER] Keyed variant — registers a named building block options instance
    /// resolvable via both <c>[FromKeyedServices("key")]</c> direct injection
    /// and <c>IOptionsSnapshot&lt;T&gt;.Get(key)</c> / <c>IOptionsMonitor&lt;T&gt;.Get(key)</c>.
    /// Following ADR-016: Supports immutable options (init) via 'with' expressions.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <param name="key">The unique key identifying this options instance.</param>
    /// <param name="transform">Optional transformation function.</param>
    /// <returns>The options instance bound at registration time.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string key,
        Func<TOptions, TOptions>? transform = null)
        where TOptions : class, IVKBlockOptions, new()
    {
        VKGuard.NotNull(services);
        VKGuard.Against(string.IsNullOrWhiteSpace(key), "Options key cannot be null or empty. Use the non-keyed overload for a single shared instance.");
        VKGuard.NotNull(configuration);
        VKGuard.Against(string.IsNullOrWhiteSpace(TOptions.SectionName), "Options SectionName cannot be null or empty.");

        // [IDEMPOTENCY CHECK] — keyed by (TOptions type, key), not just TOptions type
        var existingDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(TOptions) && key.Equals(d.ServiceKey));

        if (existingDescriptor is not null)
        {
            var existingOptions = (TOptions)existingDescriptor.KeyedImplementationInstance!;

            if (transform is null)
            {
                return existingOptions;
            }

            // ADR-016 (keyed variant): re-apply transform, replace just this key's registration
            var transformedOptions = transform(existingOptions);
            services.Remove(existingDescriptor);
            services.AddKeyedSingleton(key, transformedOptions);

            var registry = GetOrCreateRegistry<TOptions>(services);
            registry.Set(key, transformedOptions);
            return transformedOptions;
        }

        var targetConfig = configuration.GetSection(TOptions.SectionName);
        var options = targetConfig.Get<TOptions>() ?? new TOptions();

        if (transform is not null)
        {
            options = transform(options);
        }

        // 1. Keyed singleton for [FromKeyedServices("key")] direct injection
        services.AddKeyedSingleton(key, options);

        // 2. Registry + Factory for IOptions pipeline
        var reg = GetOrCreateRegistry<TOptions>(services);
        reg.Set(key, options);

        // 3. Named validation infrastructure + startup validation
        services.AddOptions<TOptions>(key).ValidateDataAnnotations().ValidateOnStart();

        return options;
    }

    /// <summary>
    /// [WRAPPER] Bridge overload for keyed <see cref="Action{TOptions}"/> configuration delegate.
    /// Converts the mutating action into the immutable <c>Func&lt;TOptions, TOptions&gt;</c> transformation pipeline per ADR-016.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The root configuration.</param>
    /// <param name="key">The unique key identifying this options instance.</param>
    /// <param name="configure">Optional standard Action-based configuration delegate.</param>
    /// <returns>The options instance bound at registration time.</returns>
    public static TOptions AddVKBlockOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string key,
        Action<TOptions>? configure)
        where TOptions : class, IVKBlockOptions, new()
    {
        Func<TOptions, TOptions>? transform = configure is null
            ? null
            : options =>
            {
                configure(options);
                return options;
            };

        return services.AddVKBlockOptions<TOptions>(
            configuration,
            key,
            transform);
    }

    private static BlockOptionsRegistry<TOptions> GetOrCreateRegistry<TOptions>(IServiceCollection services)
        where TOptions : class, IVKBlockOptions, new()
    {
        var registryDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(BlockOptionsRegistry<TOptions>));

        if (registryDescriptor?.ImplementationInstance is BlockOptionsRegistry<TOptions> existing)
        {
            return existing;
        }

        var registry = new BlockOptionsRegistry<TOptions>();
        services.AddSingleton(registry);
        services.Replace(ServiceDescriptor.Singleton<IOptionsFactory<TOptions>>(sp =>
            new BlockOptionsFactory<TOptions>(
                sp.GetRequiredService<BlockOptionsRegistry<TOptions>>(),
                sp.GetServices<IValidateOptions<TOptions>>())));

        return registry;
    }

    /// <summary>
    /// Retrieves a previously registered block options instance from the service collection.
    /// </summary>
    public static TOptions? GetVKBlockOptions<TOptions>(this IServiceCollection services) where TOptions : class
    {
        return services.FirstOrDefault(sd => sd.ServiceType == typeof(TOptions))?.ImplementationInstance as TOptions;
    }
}
