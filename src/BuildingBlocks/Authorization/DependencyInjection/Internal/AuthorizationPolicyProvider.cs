using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DependencyInjection.Internal;

/// <summary>
/// Configures semantic authorization policies by aggregating authentication schemes
/// from all registered <see cref="IVKSemanticSchemeProvider"/> implementations.
/// This achieves complete decoupling between Authentication and Authorization blocks.
/// </summary>
internal sealed class AuthorizationPolicyProvider(
    IEnumerable<IVKSemanticSchemeProvider> schemeProviders) : IConfigureOptions<AuthorizationOptions>
{
    public void Configure(AuthorizationOptions options)
    {
        // 1. Group: User
        ConfigureGroupPolicy(options, VKAuthPolicies.GroupUser, p => p.GetUserSchemes());

        // 2. Group: Service
        ConfigureGroupPolicy(options, VKAuthPolicies.GroupService, p => p.GetServiceSchemes());

        // 3. Group: Internal
        ConfigureGroupPolicy(options, VKAuthPolicies.GroupInternal, p => p.GetInternalSchemes());

        // 4. Individual Strategy Policies (Late-bound for flexibility)
        ConfigureIndividualPolicy(options, VKAuthPolicies.Jwt); // JWT typically maps to User schemes
        ConfigureIndividualPolicy(options, VKAuthPolicies.ApiKey); // ApiKey typically maps to Service schemes
        ConfigureIndividualPolicy(options, VKAuthPolicies.OAuth);  // OAuth typically maps to User or Service schemes
    }

    private void ConfigureGroupPolicy(
        AuthorizationOptions options,
        string policyName,
        Func<IVKSemanticSchemeProvider, IEnumerable<string>> schemeSelector)
    {
        var schemes = schemeProviders.SelectMany(schemeSelector).Distinct().ToList();
        if (schemes.Count != 0)
        {
            options.AddPolicy(policyName, policy =>
            {
                foreach (string? scheme in schemes)
                {
                    policy.AuthenticationSchemes.Add(scheme);
                }

                policy.RequireAuthenticatedUser();
            });
        }
    }

    private void ConfigureIndividualPolicy(AuthorizationOptions options, string policyName)
    {
        var schemes = schemeProviders.SelectMany(p => p.GetSchemesForPolicy(policyName)).Distinct().ToList();
        if (schemes.Count != 0)
        {
            options.AddPolicy(policyName, policy =>
            {
                foreach (string? scheme in schemes)
                {
                    policy.AuthenticationSchemes.Add(scheme);
                }
                policy.RequireAuthenticatedUser();
            });
        }
    }
}
