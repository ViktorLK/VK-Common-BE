using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.TenantIsolation.Internal;

namespace VK.Blocks.Authorization.Features.TenantIsolation;

internal static class TenantIsolationRegistration
{
    public static IServiceCollection AddTenantIsolationFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IUserTenantProvider, DefaultUserTenantProvider>();
        services.TryAddScoped<TenantAuthorizationHandler>();
        services.TryAddScoped<ITenantEvaluator>(sp => sp.GetRequiredService<TenantAuthorizationHandler>());
        
        return services;
    }
}
