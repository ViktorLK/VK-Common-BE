using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Providers;

namespace VK.Blocks.MultiTenancy.DependencyInjection;

/// <summary>
/// Dependency injection extensions for the MultiTenancy module.
/// </summary>
public static class MultiTenancyServiceCollectionExtensions
{
    /// <summary>
    /// Adds multi-tenancy services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<ITenantProvider, HttpContextTenantProvider>();

        return services;
    }
}
