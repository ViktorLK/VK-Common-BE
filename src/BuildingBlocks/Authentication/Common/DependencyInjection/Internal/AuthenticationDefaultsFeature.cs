using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Authentication.Common.DependencyInjection.Internal;

/// <summary>
/// Partial implementation for Authentication Defaults feature hooks.
/// Matches the inferred name 'AuthenticationDefaults' from VKAuthenticationDefaultsOptions.
/// </summary>
internal sealed partial class AuthenticationDefaultsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAuthenticationDefaultsOptions options)
    {
        _ = services;
        _ = options;
    }

    /// <summary>Add global validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKAuthenticationDefaultsOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.DefaultScheme))
        {
            failures.Add("DefaultScheme is required.");
        }

        if (options.InMemoryCleanupIntervalMinutes <= 0)
        {
            failures.Add("InMemoryCleanupIntervalMinutes must be greater than zero.");
        }
    }
}
