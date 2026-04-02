namespace VK.Blocks.Authentication.Abstractions;

/// <summary>
/// Represents user information retrieved from an OAuth provider.
/// </summary>
public sealed record ExternalIdentity
{
    #region Properties

    /// <summary>
    /// Gets the name of the OAuth provider.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user within the OAuth provider.
    /// </summary>
    public required string ProviderId { get; init; }

    /// <summary>
    /// Gets the name of the user, if available.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the email address of the user, if available.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the roles assigned to the user.
    /// </summary>
    public IReadOnlyList<string> Roles { get; init; } = [];

    /// <summary>
    /// Gets the additional claims associated with the user from the provider.
    /// Examples: "tid" (Azure Tenant ID), "hd" (Google Hosted Domain), "avatar_url" (GitHub), "picture" (Google), "locale", etc.
    /// </summary>
    public IReadOnlyDictionary<string, string> Claims { get; init; } = new Dictionary<string, string>();

    #endregion
}
