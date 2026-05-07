using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect.DependencyInjection.Internal;

/// <summary>
/// Builder for the OpenIdConnect building block.
/// Complies with BB.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Simple property bag for DI registration; contains no complex logic.")]
internal sealed class OidcBlockBuilder : IVKBlockBuilder<VKAuthenticationBlock>
{
    public OidcBlockBuilder(IServiceCollection services, IConfiguration configuration)
    {
        Services = VKGuard.NotNull(services);
        Configuration = VKGuard.NotNull(configuration);
    }

    public IServiceCollection Services { get; }
    public IConfiguration Configuration { get; }
}

