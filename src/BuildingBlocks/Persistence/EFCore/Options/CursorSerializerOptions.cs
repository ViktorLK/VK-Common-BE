namespace VK.Blocks.Persistence.EFCore.Options;

/// <summary>
/// Configuration options for <see cref="Infrastructure.SecureCursorSerializer"/>.
/// Bind this from <c>appsettings.json</c> under the <c>"CursorSerializer"</c> section.
/// </summary>
/// <example>
/// <code>
/// {
///   "CursorSerializer": {
///     "SigningKey": "your-secret-key-from-keyvault",
///     "DefaultExpiry": "01:00:00"
///   }
/// }
/// </code>
/// </example>
public sealed class CursorSerializerOptions
{
    #region Constants

    /// <summary>
    /// The configuration section name used for binding.
    /// </summary>
    public const string SectionName = "CursorSerializer";

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the HMAC signing key.
    /// In production, retrieve this from a secrets manager (e.g., Azure Key Vault).
    /// </summary>
    public string? SigningKey { get; set; }

    /// <summary>
    /// Gets or sets the default expiry duration for cursor tokens.
    /// Set to <c>null</c> for tokens that never expire.
    /// Defaults to 1 hour.
    /// </summary>
    public TimeSpan? DefaultExpiry { get; set; } = TimeSpan.FromHours(1);

    #endregion
}
