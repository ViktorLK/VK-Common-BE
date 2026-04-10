using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.Permissions.Internal;

namespace VK.Blocks.Authorization.Features.Permissions;

internal static class PermissionsRegistration
{
    public static IServiceCollection AddPermissionsFeature(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPermissionProvider, DefaultPermissionProvider>());
        services.TryAddScoped<PermissionHandler>();
        services.TryAddScoped<IPermissionEvaluator>(sp => sp.GetRequiredService<PermissionHandler>());
        
        return services;
    }
}
