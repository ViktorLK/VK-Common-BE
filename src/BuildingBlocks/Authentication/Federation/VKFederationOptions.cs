using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Configuration options for Identity Federation and Account Linking.
/// </summary>
public sealed partial record VKFederationOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether identity federation is enabled.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the schema name for identity federation.
    /// </summary>
    public string SchemeName { get; init; } = "IdentityFederation";

    /// <summary>
    /// Gets or sets a value indicating whether to allow mapping a single external identity to multiple local user accounts.
    /// Default is false (strictly one local account per external credentials).
    /// </summary>
    public bool AllowMultipleLinksPerExternalAccount { get; init; } = false;
}
