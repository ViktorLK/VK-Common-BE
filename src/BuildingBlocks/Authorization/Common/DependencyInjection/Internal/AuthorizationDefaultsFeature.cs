using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Authorization.Common.DependencyInjection.Internal;

/// <summary>
/// Hook implementation for the generated AuthorizationDefaultsFeature.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed partial class AuthorizationDefaultsFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAuthorizationDefaultsOptions options)
    {
        // No custom services to register for defaults
    }

    static partial void ValidateFeatureCustom(VKAuthorizationDefaultsOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.RoleClaimType))
        {
            failures.Add("RoleClaimType cannot be null or whitespace. This is required for SuperAdmin bypass evaluation.");
        }

        if (options.SuperAdminRole is not null && string.IsNullOrWhiteSpace(options.SuperAdminRole))
        {
            failures.Add("SuperAdminRole must be null or a non-whitespace string.");
        }

        if (string.IsNullOrWhiteSpace(options.TenantClaimType))
        {
            failures.Add("TenantClaimType cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(options.RankClaimType))
        {
            failures.Add("RankClaimType cannot be null or whitespace.");
        }
    }
}
