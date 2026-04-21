namespace VK.Blocks.Authentication;

/// <summary>
/// Centralized constants for the authentication module to avoid magic strings.
/// </summary>
public static class VKAuthenticationConstants
{
    /// <summary>
    /// The default authentication policy name for JWT tokens.
    /// </summary>
    public const string JwtPolicy = "VK.Jwt";

    /// <summary>
    /// The default authentication policy name for API keys.
    /// </summary>
    public const string ApiKeyPolicy = "VK.ApiKey";

    /// <summary>
    /// Prefix for grouped authentication policies.
    /// </summary>
    public const string GroupPolicyPrefix = "VK.Group.";

    /// <summary>
    /// Content type for RFC 7807 problem details responses.
    /// </summary>
    public const string ProblemJsonContentType = "application/problem+json";

    /// <summary>
    /// Standard title for unauthorized responses.
    /// </summary>
    public const string UnauthorizedTitle = "Unauthorized";

    /// <summary>
    /// The field name for the trace identifier in problem details responses.
    /// </summary>
    public const string TraceIdExtension = "traceId";

    /// <summary>
    /// Validation message for missing default authentication scheme.
    /// </summary>
    public const string DefaultSchemeRequired = "DefaultScheme must be specified.";

    /// <summary>
    /// Validation message for invalid in-memory cleanup interval.
    /// </summary>
    public const string MinCleanupIntervalId = "InMemoryCleanupIntervalMinutes must be at least 1.";

    /// <summary>
    /// Validation message for missing OAuth authority.
    /// </summary>
    public const string OAuthAuthorityRequired = "OAuth provider '{0}' must have a valid absolute Authority URL.";

    /// <summary>
    /// Validation message for missing OAuth ClientId.
    /// </summary>
    public const string OAuthClientIdRequired = "OAuth provider '{0}' must have a ClientId.";

    /// <summary>
    /// Validation message for invalid OAuth callback path.
    /// </summary>
    public const string OAuthCallbackPathInvalid = "OAuth provider '{0}' CallbackPath must start with a forward slash '/'.";

    /// <summary>
    /// Validation message templates for authentication strategies.
    /// </summary>
    public const string JwtValidationFailedTemplate = "JWT authentication is enabled but misconfigured. Scheme: {0}";
    public const string OAuthValidationFailedTemplate = "OAuth authentication is enabled but misconfigured. Section: {0}";
    public const string ApiKeyValidationFailedTemplate = "API Key authentication is enabled but misconfigured. Header: {0}";

    /// <summary>
    /// Validation message when no authentication strategies are enabled.
    /// </summary>
    public const string AtLeastOneStrategyRequired = "Authentication block is enabled, but no authentication strategies (JWT, ApiKey, or OAuth) are enabled. Enable at least one strategy or set 'Authentication:Enabled' to false.";
}
