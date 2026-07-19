using System;

namespace VK.Blocks.Authentication;

/// <summary>
/// Represents mapping details between a local user account and an external identity.
/// </summary>
public sealed record VKAccountLinkInfo
{
    /// <summary>
    /// Gets the local system user identifier.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Gets the external login provider name (e.g. "Google", "GitHub").
    /// </summary>
    public required string LoginProvider { get; init; }

    /// <summary>
    /// Gets the external identity provider's unique key for the user.
    /// </summary>
    public required string ProviderKey { get; init; }

    /// <summary>
    /// Gets the display name of the external login provider profile.
    /// </summary>
    public required string ProviderDisplayName { get; init; }

    /// <summary>
    /// Gets the date and time when the account was linked.
    /// </summary>
    public DateTimeOffset LinkedAt { get; init; }
}
