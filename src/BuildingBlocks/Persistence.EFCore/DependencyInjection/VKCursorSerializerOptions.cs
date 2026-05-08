using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Configuration options for the secure cursor serializer.
/// Bind this from <c>appsettings.json</c> under the persistence block section.
/// </summary>
public sealed record VKCursorSerializerOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Persistence:CursorSerializer";

    /// <summary>
    /// Gets the HMAC signing key.
    /// In production, retrieve this from a secrets manager (e.g., Azure Key Vault).
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MinLength(32)] // 256 bits recommended for HMAC
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets the default expiry duration for cursor tokens.
    /// Set to <c>null</c> for tokens that never expire.
    /// Defaults to 1 hour.
    /// </summary>
    public TimeSpan? DefaultExpiry { get; init; } = TimeSpan.FromHours(1);
}
