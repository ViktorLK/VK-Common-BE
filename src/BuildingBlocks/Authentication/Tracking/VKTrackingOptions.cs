using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Configuration options for tracking login states and client device fingerprints.
/// </summary>
public sealed partial record VKTrackingOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether login tracking is enabled.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the schema name for login tracking.
    /// </summary>
    public string SchemeName { get; init; } = "LoginTracking";

    /// <summary>
    /// Gets or sets the HTTP header name used to pass client device fingerprints.
    /// Default is "X-Device-Fingerprint".
    /// </summary>
    public string FingerprintHeaderName { get; init; } = "X-Device-Fingerprint";

    /// <summary>
    /// Gets or sets a value indicating whether to log a warning when device fingerprint header is missing.
    /// </summary>
    public bool WarnOnMissingFingerprint { get; init; } = true;
}
