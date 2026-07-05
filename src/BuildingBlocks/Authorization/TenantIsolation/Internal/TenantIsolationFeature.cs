using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.TenantIsolation.Internal;

[ExcludeFromCodeCoverage]
internal sealed partial class TenantIsolationFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTenantIsolationOptions options)
    {
        services.TryAddScoped<IVKUserTenantProvider, DefaultUserTenantProvider>();
        services.TryAddScoped<TenantAuthorizationHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, TenantAuthorizationHandler>();
        services.TryAddScopedForwarding<IVKTenantEvaluator, TenantAuthorizationHandler>();
    }

    static partial void ValidateFeatureCustom(VKTenantIsolationOptions options, List<string> failures)
    {
        if (options.TenantClaimType is not null && string.IsNullOrWhiteSpace(options.TenantClaimType))
        {
            failures.Add("TenantClaimType cannot be whitespace if provided.");
        }
    }
}
