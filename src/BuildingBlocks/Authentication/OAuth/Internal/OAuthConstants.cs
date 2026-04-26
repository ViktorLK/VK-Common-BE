namespace VK.Blocks.Authentication.OAuth.Internal;

/// <summary>
/// Contains constants used by the OAuth infrastructure.
/// </summary>
internal static class OAuthConstants
{
    /// <summary>
    /// The name of the OAuth feature.
    /// </summary>
    internal const string FeatureName = "OAuth";

    /// <summary>
    /// Identifier for the GitHub provider.
    /// </summary>
    internal const string GitHub = "GitHub";

    /// <summary>
    /// Message for missing ClientId.
    /// </summary>
    internal const string ClientIdRequired = "OAuth provider '{0}' is enabled but missing ClientId.";

    /// <summary>
    /// Message for missing Authority.
    /// </summary>
    internal const string AuthorityRequired = "OAuth provider '{0}' is enabled but missing Authority.";
}
