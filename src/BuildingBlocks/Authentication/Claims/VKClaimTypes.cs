namespace VK.Blocks.Authentication.Claims;

/// <summary>
/// Standardized claim types used across VK system.
/// </summary>
public static class VKClaimTypes
{
    #region Fields

    /// <summary>
    /// The claim type for the tenant identifier.
    /// </summary>
    public const string TenantId = "vk.tenant.id";

    /// <summary>
    /// The claim type for the organization identifier.
    /// </summary>
    public const string OrganizationId = "vk.org.id";

    /// <summary>
    /// The claim type for the user identifier.
    /// </summary>
    public const string UserId = "vk.user.id";

    /// <summary>
    /// The claim type for user permissions.
    /// </summary>
    public const string Permissions = "vk.permissions";

    /// <summary>
    /// The claim type for the session identifier.
    /// </summary>
    public const string SessionId = "vk.session.id";

    /// <summary>
    /// The claim type for the authorization scope.
    /// </summary>
    public const string Scope = "vk.scope";

    /// <summary>
    /// The claim type for the authentication type.
    /// </summary>
    public const string AuthType = "vk.auth.type";

    /// <summary>
    /// The claim type for the API key identifier.
    /// </summary>
    public const string KeyId = "vk.key.id";

    /// <summary>
    /// The claim type for the user's avatar URL.
    /// </summary>
    public const string AvatarUrl = "vk.user.avatar";

    /// <summary>
    /// The claim type for the user's locale.
    /// </summary>
    public const string Locale = "vk.user.locale";

    /// <summary>
    /// The claim type for the user's profile URL.
    /// </summary>
    public const string ProfileUrl = "vk.user.profile";

    /// <summary>
    /// The claim type for the B2C Trust Framework Policy (User Flow).
    /// </summary>
    public const string TrustFrameworkPolicy = "vk.b2c.tfp";

    #endregion
}
