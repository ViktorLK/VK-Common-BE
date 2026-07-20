using VK.Blocks.Authorization.Roles;
using VK.Blocks.Authorization.Roles.Internal;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

[ExcludeFromCodeCoverage]
[VKFeature(typeof(VKAuthorizationBlock), "Roles", OptionsType = typeof(VKRoleOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class RolesFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKRoleOptions options)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKRoleProvider, DefaultRoleProvider>());
        services.TryAddScoped<RoleHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, RoleHandler>();
        services.TryAddScopedForwarding<IVKRoleEvaluator, RoleHandler>();
    }

    static partial void ValidateFeatureCustom(VKRoleOptions options, List<string> failures)
    {
        if (options.RoleClaimType is not null && string.IsNullOrWhiteSpace(options.RoleClaimType))
        {
            failures.Add("RoleClaimType cannot be whitespace if provided.");
        }
    }
}
