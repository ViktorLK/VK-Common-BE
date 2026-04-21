using Microsoft.AspNetCore.Authentication;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Defines a builder for configuring authentication services within the VK.Blocks framework.
/// </summary>
public interface IVKAuthenticationBuilder : IVKBlockBuilder<AuthenticationBlock>
{
    /// <summary>
    /// Gets the underlying ASP.NET Core authentication builder.
    /// </summary>
    AuthenticationBuilder AuthBuilder { get; }
}
