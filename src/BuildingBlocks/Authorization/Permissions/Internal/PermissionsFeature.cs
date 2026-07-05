using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Permissions.Internal;

[ExcludeFromCodeCoverage]
internal sealed partial class PermissionsFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKPermissionOptions options)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPermissionProvider, DefaultPermissionProvider>());
        services.TryAddScoped<PermissionHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, PermissionHandler>();
        services.TryAddScopedForwarding<IVKPermissionEvaluator, PermissionHandler>();
    }

    static partial void ValidateFeatureCustom(VKPermissionOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.PermissionClaimType))
        {
            failures.Add("PermissionClaimType cannot be null or whitespace.");
        }
    }
}
