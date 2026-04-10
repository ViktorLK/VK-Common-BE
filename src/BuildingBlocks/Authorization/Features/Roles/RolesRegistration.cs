using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.Roles.Internal;

namespace VK.Blocks.Authorization.Features.Roles;

internal static class RolesRegistration
{
    public static IServiceCollection AddRolesFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IRoleProvider, DefaultRoleProvider>();
        services.TryAddScoped<RoleHandler>();
        services.TryAddScoped<IRoleEvaluator>(sp => sp.GetRequiredService<RoleHandler>());
        
        return services;
    }
}
