using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.Permissions.Internal;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Provides extension methods for registering the Permissions authorization feature.
/// </summary>
internal static class PermissionsRegistration
{
    /// <summary>
    /// Adds the Permissions authorization feature to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPermissionsFeature(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPermissionProvider, DefaultPermissionProvider>());
        services.TryAddScoped<PermissionHandler>();
        services.TryAddScoped<IPermissionEvaluator>(sp => sp.GetRequiredService<PermissionHandler>());

        return services;
    }
}
