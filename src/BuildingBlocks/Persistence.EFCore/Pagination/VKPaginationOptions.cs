using System;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Configuration options for the pagination feature.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>

public sealed partial record VKPaginationOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to use the secure cursor serializer.
    /// </summary>
    public bool UseSecureSerializer { get; init; } = false;

    /// <summary>
    /// Gets the HMAC signing key.
    /// In production, retrieve this from a secrets manager (e.g., Azure Key Vault).
    /// </summary>
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets the default expiry duration for cursor tokens.
    /// Set to <c>null</c> for tokens that never expire.
    /// Defaults to 1 hour.
    /// </summary>
    public TimeSpan? DefaultExpiry { get; init; } = TimeSpan.FromHours(1);
}
