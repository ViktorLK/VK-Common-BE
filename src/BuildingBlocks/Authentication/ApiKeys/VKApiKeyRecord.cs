using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Represents a stored API key record.
/// </summary>
public sealed record VKApiKeyRecord : IVKMultiTenant
{
    /// <summary>
    /// Gets the unique identifier of the API key record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the owner identifier of the API key.
    /// </summary>
    public required string OwnerId { get; init; }

    /// <inheritdoc />
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the API key is currently enabled.
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Gets the expiration date and time of the API key, if applicable.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Gets the scopes associated with the API key.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; init; } = [];
}
