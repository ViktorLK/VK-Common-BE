using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FrameworkAuthenticationBuilder = Microsoft.AspNetCore.Authentication.AuthenticationBuilder;

namespace VK.Blocks.Authentication.DependencyInjection.Internal;

/// <summary>
/// Default implementation of the authentication builder.
/// </summary>
internal sealed class AuthenticationBlockBuilder(
    IServiceCollection services,
    IConfiguration configuration,
    FrameworkAuthenticationBuilder authBuilder) : IVKAuthenticationBuilder
{
    public IServiceCollection Services { get; } = services;
    public IConfiguration Configuration { get; } = configuration;
    public FrameworkAuthenticationBuilder AuthBuilder { get; } = authBuilder;
}




