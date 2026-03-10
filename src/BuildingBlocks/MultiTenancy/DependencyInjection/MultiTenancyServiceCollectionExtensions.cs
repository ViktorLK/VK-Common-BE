using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Context;
using VK.Blocks.MultiTenancy.Options;
using VK.Blocks.MultiTenancy.Resolution;
using VK.Blocks.MultiTenancy.Resolution.Resolvers;

namespace VK.Blocks.MultiTenancy.DependencyInjection;

/// <summary>
/// Dependency injection extensions for the MultiTenancy module.
/// </summary>
public static class MultiTenancyServiceCollectionExtensions
{
    /// <summary>
    /// Adds multi-tenancy services to the specified <see cref="IServiceCollection"/>,
    /// including the resolution pipeline, resolvers, and tenant context.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional delegate to configure <see cref="MultiTenancyOptions"/>.</param>
    /// <param name="configureResolution">Optional delegate to configure <see cref="TenantResolutionOptions"/>.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddMultiTenancy(
        this IServiceCollection services,
        Action<MultiTenancyOptions>? configureOptions = null,
        Action<TenantResolutionOptions>? configureResolution = null)
    {
        // Configure options
        var multiTenancyOptions = new MultiTenancyOptions();
        configureOptions?.Invoke(multiTenancyOptions);
        services.Configure<MultiTenancyOptions>(options =>
        {
            options.EnforceTenancy = multiTenancyOptions.EnforceTenancy;
            options.EnabledResolvers = multiTenancyOptions.EnabledResolvers;
        });

        var resolutionOptions = new TenantResolutionOptions();
        configureResolution?.Invoke(resolutionOptions);
        services.AddSingleton(resolutionOptions);

        // Core services
        services.AddHttpContextAccessor();
        services.TryAddScoped<ITenantContext, TenantContext>();
        services.TryAddScoped<TenantContext>();
        services.TryAddSingleton<TenantContextAccessor>();
        services.TryAddScoped<TenantResolutionPipeline>();

        // ITenantProvider backed by TenantContext
        services.TryAddScoped<ITenantProvider, TenantContextTenantProvider>();

        // Register resolvers based on configuration
        RegisterResolvers(services, multiTenancyOptions);

        return services;
    }

    /// <summary>
    /// Adds the tenant resolution middleware to the ASP.NET Core pipeline.
    /// This must be called after authentication middleware if using <see cref="ClaimsTenantResolver"/>.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The modified application builder.</returns>
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }

    #region Private Methods

    private static void RegisterResolvers(
        IServiceCollection services,
        MultiTenancyOptions options)
    {
        var enabledResolvers = options.EnabledResolvers;
        var registerAll = enabledResolvers.Count == 0;

        if (registerAll || enabledResolvers.Contains(TenantResolverType.Header))
        {
            services.AddScoped<ITenantResolver, HeaderTenantResolver>();
        }

        if (registerAll || enabledResolvers.Contains(TenantResolverType.Claims))
        {
            services.AddScoped<ITenantResolver, ClaimsTenantResolver>();
        }

        if (registerAll || enabledResolvers.Contains(TenantResolverType.Domain))
        {
            services.AddScoped<ITenantResolver, DomainTenantResolver>();
        }

        if (registerAll || enabledResolvers.Contains(TenantResolverType.QueryString))
        {
            services.AddScoped<ITenantResolver, QueryStringTenantResolver>();
        }
    }

    #endregion
}
