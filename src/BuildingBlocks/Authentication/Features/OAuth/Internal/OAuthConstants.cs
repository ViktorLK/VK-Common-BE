namespace VK.Blocks.Authentication.Features.OAuth.Internal;

/// <summary>
/// Contains constants used by the OAuth infrastructure.
/// </summary>
internal static class OAuthConstants
{
    /// <summary>
    /// Identifier for the GitHub provider.
    /// </summary>
    public const string GitHub = "GitHub";

    /// <summary>
    /// Message for missing ClientId.
    /// </summary>
    public const string ClientIdRequired = "OAuth provider '{0}' is enabled but missing ClientId.";

    /// <summary>
    /// Message for missing Authority.
    /// </summary>
    public const string AuthorityRequired = "OAuth provider '{0}' is enabled but missing Authority.";
}
