namespace VK.Blocks.Authentication.Features.OAuth;

/// <summary>
/// Contains standardized error messages for OAuth configuration and validation.
/// </summary>
public static class VKOAuthErrors
{
    public const string MissingProviders = "At least one OAuth provider must be configured when OAuth is enabled.";
    
    public const string MissingClientIdTemplate = "OAuth provider '{0}' is missing ClientId.";
    
    public const string MissingClientSecretTemplate = "OAuth provider '{0}' is missing ClientSecret.";
    
    public const string MissingAuthorityTemplate = "OAuth provider '{0}' is missing Authority.";
    
    public const string MissingCallbackPathTemplate = "OAuth provider '{0}' is missing CallbackPath.";
}
