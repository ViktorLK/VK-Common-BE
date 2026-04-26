using Microsoft.AspNetCore.Authentication;

namespace VK.Blocks.Authentication.ApiKeys.Internal;

/// <summary>
/// Options for configuring API key authentication.
/// </summary>
internal sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Gets or sets the HTTP header name that contains the API key.
    /// </summary>
    public string HeaderName { get; set; } = ApiKeyConstants.DefaultHeaderName;

    /// <summary>
    /// Gets or sets the authentication type associated with the API key.
    /// </summary>
    public string AuthType { get; set; } = ApiKeyConstants.DefaultAuthType;
}
