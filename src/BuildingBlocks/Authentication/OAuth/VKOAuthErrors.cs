using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Errors associated with OAuth authentication.
/// </summary>
public static class VKOAuthErrors
{
    /// <summary>
    /// VKError when a requested OAuth provider is not supported by the system.
    /// </summary>
    public static readonly VKError ProviderNotSupported = new("OAuth.ProviderNotSupported", "The OAuth provider is not supported.", VKErrorType.Failure);

    /// <summary>
    /// VKError when the OAuth state is invalid or has expired.
    /// </summary>
    public static readonly VKError InvalidState = new("OAuth.InvalidState", "The OAuth state is invalid or has expired.", VKErrorType.Failure);

    /// <summary>
    /// VKError representing a generic failure from the remote identity provider.
    /// </summary>
    public static readonly VKError RemoteFailure = new("OAuth.RemoteFailure", "VKError from external identity provider.", VKErrorType.Failure);

    /// <summary>
    /// VKError when the external provider returns an identity missing critical claims.
    /// </summary>
    public static readonly VKError MissingRequiredClaim = new("OAuth.MissingRequiredClaim", "The external provider did not return the required claims.", VKErrorType.Failure);

    /// <summary>
    /// VKError when no claims mapper is registered for the specified provider.
    /// </summary>
    public static readonly VKError MapperNotFound = new("OAuth.MapperNotFound", "No claims mapper found for the specified provider.", VKErrorType.Failure);

    /// <summary>
    /// VKError when user info retrieval from the external provider fails.
    /// </summary>
    public static readonly VKError UserInfoFailure = new("OAuth.UserInfoFailure", "Failed to retrieve user information from the external provider.", VKErrorType.Failure);

    /// <summary>
    /// VKError message when OAuth is enabled but no providers are configured.
    /// </summary>
    public const string MissingProviders = "At least one OAuth provider must be configured when OAuth is enabled.";

    /// <summary>
    /// VKError message template when an OAuth provider is missing ClientId.
    /// </summary>
    public const string MissingClientIdTemplate = "OAuth provider '{0}' is missing ClientId.";

    /// <summary>
    /// VKError message template when an OAuth provider is missing ClientSecret.
    /// </summary>
    public const string MissingClientSecretTemplate = "OAuth provider '{0}' is missing ClientSecret.";

    /// <summary>
    /// VKError message template when an OAuth provider is missing Authority.
    /// </summary>
    public const string MissingAuthorityTemplate = "OAuth provider '{0}' is missing Authority.";

    /// <summary>
    /// VKError message template when an OAuth provider is missing CallbackPath.
    /// </summary>
    public const string MissingCallbackPathTemplate = "OAuth provider '{0}' is missing CallbackPath.";
}
