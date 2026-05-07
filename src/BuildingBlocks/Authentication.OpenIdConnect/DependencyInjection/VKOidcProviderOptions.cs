using System.Collections.Generic;

namespace VK.Blocks.Authentication.OpenIdConnect;

/// <summary>
/// Represents the configuration options for a single OIDC provider.
/// </summary>
public sealed record VKOidcProviderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether this provider is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the custom authentication scheme name.
    /// If not provided, the configuration key (e.g. "Google") will be used.
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
    /// Gets the requested scopes.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; init; } = [];

    /// <summary>
    /// Gets or sets the response type (e.g. "code", "id_token").
    /// Defaults to "code".
    /// </summary>
    public string? ResponseType { get; init; } = "code";

    /// <summary>
    /// Gets or sets a value indicating whether to retrieve additional claims from the user info endpoint.
    /// </summary>
    public bool GetClaimsFromUserInfoEndpoint { get; init; } = true;
}
