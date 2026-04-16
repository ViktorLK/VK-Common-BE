using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.TenantIsolation.Internal;

namespace VK.Blocks.Authorization.Features.TenantIsolation;

/// <summary>
/// Provides extension methods for registering the Tenant Isolation authorization feature.
/// </summary>
internal static class TenantIsolationRegistration
{
    /// <summary>
    /// Adds the Tenant Isolation authorization feature to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddTenantIsolationFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IUserTenantProvider, DefaultUserTenantProvider>();
        services.TryAddScoped<TenantAuthorizationHandler>();
        services.TryAddScoped<ITenantEvaluator>(sp => sp.GetRequiredService<TenantAuthorizationHandler>());

        return services;
    }
}
