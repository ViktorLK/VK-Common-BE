using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Globally shared errors for the authentication module.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static error definitions and constant error descriptors.")]
public static class VKAuthenticationErrors
{
    /// <summary>
    /// VKError returned when mandatory claims are missing from a principal.
    /// </summary>
    public static readonly VKError InvalidClaims = new("Auth.InvalidClaims", "One or more required claims are missing from the principal.", VKErrorType.Unauthorized);

    /// <summary>
    /// VKError returned when a tenant identifier is required but missing or invalid.
    /// </summary>
    public static readonly VKError TenantIsolationFailed = new("Auth.TenantIsolationFailed", "Tenant isolation check failed: Tenant identifier is missing or invalid.", VKErrorType.Forbidden);
}
