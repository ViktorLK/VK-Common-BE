using System.Collections.Generic;

namespace VK.Blocks.Authentication;

/// <summary>
/// Represents the configuration options for a single OAuth provider.
/// </summary>
public sealed record VKOAuthProviderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether this provider is enabled.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Gets or sets the custom authentication scheme name.
    /// If not provided, the configuration key (e.g. "GitHub") will be used.
    /// </summary>
    public string? SchemeName { get; init; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public required string ClientSecret { get; init; }

    /// <summary>
    /// Gets or sets the authority URL.
    /// </summary>
    public required string Authority { get; init; }

    /// <summary>
    /// Gets or sets the callback path.
    /// </summary>
    public required string CallbackPath { get; init; }

    /// <summary>
    /// Gets or sets the requested scopes.
    /// </summary>
    public List<string> Scopes { get; init; } = [];

    /// <summary>
    /// Gets or sets the response type (e.g. "code", "id_token").
    /// Defaults to "code" in OIDC if not specified.
    /// </summary>
    public string? ResponseType { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to retrieve additional claims from the user info endpoint.
    /// Necessary for providers that don't include all profile data in the ID Token (e.g. GitHub).
    /// </summary>
    public bool GetClaimsFromUserInfoEndpoint { get; init; }
}




