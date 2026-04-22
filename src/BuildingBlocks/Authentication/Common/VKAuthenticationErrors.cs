using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Globally shared errors for the authentication module.
/// </summary>
public static class VKAuthenticationErrors
{
    /// <summary>
    /// VKError returned when mandatory claims are missing from a principal.
    /// </summary>
    public static readonly VKError InvalidClaims = new("Auth.InvalidClaims", "One or more required claims are missing from the principal.", VKErrorType.Unauthorized);
}
