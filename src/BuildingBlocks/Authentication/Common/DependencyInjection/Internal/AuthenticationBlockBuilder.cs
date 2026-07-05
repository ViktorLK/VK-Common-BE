using Microsoft.AspNetCore.Authentication;

namespace VK.Blocks.Authentication.Common.DependencyInjection.Internal;

/// <summary>
/// Default implementation of the authentication builder.
/// Manual partial class to implement AuthBuilder property.
/// </summary>
// [SG Builder]
internal sealed partial class AuthenticationBlockBuilder
{
    /// <summary>
    /// Gets or sets the inner ASP.NET Core AuthenticationBuilder.
    /// </summary>
    public AuthenticationBuilder AuthBuilder { get; set; } = null!;
}
