using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.DependencyInjection.Internal;

/// <summary>
/// Default implementation of the authentication builder.
/// </summary>
internal sealed class AuthenticationBlockBuilder(
    IServiceCollection services,
    IConfiguration configuration,
    AuthenticationBuilder authBuilder)
    : VKBlockBuilder<VKAuthenticationBlock>(services, configuration), IVKAuthenticationBuilder
{
    public AuthenticationBuilder AuthBuilder { get; } = authBuilder;
}
