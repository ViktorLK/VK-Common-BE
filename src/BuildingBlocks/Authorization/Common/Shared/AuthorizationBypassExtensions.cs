using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authorization.Common.Shared;

/// <summary>
/// Provides extension methods for handling fail-open bypass logic across authorization handlers.
/// </summary>
internal static class AuthorizationBypassExtensions
{
    /// <summary>
    /// Checks if the failed policy evaluation should fail-open based on configured options.
    /// </summary>
    public static bool ShouldFailOpen(
        this VKAuthorizationOptions options,
        string policyName,
        ILogger logger)
    {
        if (options.FailOpenPolicies is null || options.FailOpenPolicies.Length == 0)
        {
            return false;
        }

        var match = options.FailOpenPolicies.Any(p => string.Equals(p, policyName, StringComparison.OrdinalIgnoreCase));
        if (match)
        {
            logger.LogWarning("Authorization policy '{PolicyName}' failed but was bypassed due to Fail-Open configuration.", policyName);
            return true;
        }

        return false;
    }
}
