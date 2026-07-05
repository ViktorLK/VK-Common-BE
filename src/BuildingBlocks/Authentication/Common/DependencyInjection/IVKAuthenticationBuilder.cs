using Microsoft.AspNetCore.Authentication;

namespace VK.Blocks.Authentication;

/// <summary>
/// Defines a builder contract for configuring VK.Blocks Authentication.
/// Manual partial interface to expose AuthBuilder.
/// </summary>
// [SG Builder]
public partial interface IVKAuthenticationBuilder
{
    /// <summary>
    /// Gets or sets the inner ASP.NET Core AuthenticationBuilder.
    /// </summary>
    AuthenticationBuilder AuthBuilder { get; set; }
}
