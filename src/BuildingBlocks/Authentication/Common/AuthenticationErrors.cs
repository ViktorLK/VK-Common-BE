using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Common;

/// <summary>
/// Globally shared errors for the authentication module.
/// </summary>
public static class AuthenticationErrors
{
    /// <summary>
    /// Error returned when mandatory claims are missing from a principal.
    /// </summary>
    public static readonly Error InvalidClaims = new("Auth.InvalidClaims", "One or more required claims are missing from the principal.", ErrorType.Unauthorized);
}
