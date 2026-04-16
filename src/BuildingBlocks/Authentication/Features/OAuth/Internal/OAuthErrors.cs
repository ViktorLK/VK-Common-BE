using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Features.OAuth.Internal;

/// <summary>
/// Errors associated with OAuth authentication.
/// </summary>
public static class OAuthErrors
{
    /// <summary>
    /// Error when a requested OAuth provider is not supported by the system.
    /// </summary>
    public static readonly Error ProviderNotSupported = new("OAuth.ProviderNotSupported", "The OAuth provider is not supported.", ErrorType.Failure);

    /// <summary>
    /// Error when the OAuth state is invalid or has expired.
    /// </summary>
    public static readonly Error InvalidState = new("OAuth.InvalidState", "The OAuth state is invalid or has expired.", ErrorType.Failure);

    /// <summary>
    /// Error representing a generic failure from the remote identity provider.
    /// </summary>
    public static readonly Error RemoteFailure = new("OAuth.RemoteFailure", "Error from external identity provider.", ErrorType.Failure);

    /// <summary>
    /// Error when the external provider returns an identity missing critical claims.
    /// </summary>
    public static readonly Error MissingRequiredClaim = new("OAuth.MissingRequiredClaim", "The external provider did not return the required claims.", ErrorType.Failure);

    /// <summary>
    /// Error when no claims mapper is registered for the specified provider.
    /// </summary>
    public static readonly Error MapperNotFound = new("OAuth.MapperNotFound", "No claims mapper found for the specified provider.", ErrorType.Failure);

    /// <summary>
    /// Error when user info retrieval from the external provider fails.
    /// </summary>
    public static readonly Error UserInfoFailure = new("OAuth.UserInfoFailure", "Failed to retrieve user information from the external provider.", ErrorType.Failure);
}
