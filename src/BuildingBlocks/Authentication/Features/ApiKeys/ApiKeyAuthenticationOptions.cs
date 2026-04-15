using Microsoft.AspNetCore.Authentication;

namespace VK.Blocks.Authentication.Features.ApiKeys;

/// <summary>
/// Options for configuring API key authentication.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Gets or sets the HTTP header name that contains the API key.
    /// </summary>
    public string HeaderName { get; set; } = "X-Api-Key";

    /// <summary>
    /// Gets or sets the authentication type associated with the API key.
    /// </summary>
    public string AuthType { get; set; } = "ApiKey";
}
