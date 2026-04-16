namespace VK.Blocks.Authentication.Features.OAuth.Internal;

/// <summary>
/// Contains standardized error messages for OAuth configuration and validation.
/// </summary>
public static class VKOAuthErrors
{
    /// <summary>
    /// Error message when OAuth is enabled but no providers are configured.
    /// </summary>
    public const string MissingProviders = "At least one OAuth provider must be configured when OAuth is enabled.";
    
    /// <summary>
    /// Error message template when an OAuth provider is missing ClientId.
    /// </summary>
    public const string MissingClientIdTemplate = "OAuth provider '{0}' is missing ClientId.";
    
    /// <summary>
    /// Error message template when an OAuth provider is missing ClientSecret.
    /// </summary>
    public const string MissingClientSecretTemplate = "OAuth provider '{0}' is missing ClientSecret.";
    
    /// <summary>
    /// Error message template when an OAuth provider is missing Authority.
    /// </summary>
    public const string MissingAuthorityTemplate = "OAuth provider '{0}' is missing Authority.";
    
    /// <summary>
    /// Error message template when an OAuth provider is missing CallbackPath.
    /// </summary>
    public const string MissingCallbackPathTemplate = "OAuth provider '{0}' is missing CallbackPath.";
}
