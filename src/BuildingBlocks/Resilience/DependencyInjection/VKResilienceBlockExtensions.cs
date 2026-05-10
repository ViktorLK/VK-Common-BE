using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Resilience.DependencyInjection.Internal;

namespace VK.Blocks.Resilience;

/// <summary>
/// Provides extension methods for registering the Resilience building block.
/// </summary>
public static class VKResilienceBlockExtensions
{
    /// <summary>
    /// Adds the Resilience building block to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The resilience builder.</returns>
    public static IVKResilienceBuilder AddResilienceBlock(this IServiceCollection services, IConfiguration configuration)
        => ResilienceBlockRegistration.Register(services, configuration);
}
