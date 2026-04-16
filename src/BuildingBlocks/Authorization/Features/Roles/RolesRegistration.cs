using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.Roles.Internal;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// Provides extension methods for registering the Roles authorization feature.
/// </summary>
internal static class RolesRegistration
{
    /// <summary>
    /// Adds the Roles authorization feature to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRolesFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IRoleProvider, DefaultRoleProvider>();
        services.TryAddScoped<RoleHandler>();
        services.TryAddScoped<IRoleEvaluator>(sp => sp.GetRequiredService<RoleHandler>());

        return services;
    }
}
