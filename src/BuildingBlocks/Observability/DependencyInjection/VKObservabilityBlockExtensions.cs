using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Observability.DependencyInjection.Internal;

namespace VK.Blocks.Observability;

/// <summary>
/// Public entry point for the Observability building block.
/// Complies with BB.03.1.
/// </summary>
public static class VKObservabilityBlockExtensions
{
    /// <summary>
    /// Adds the Observability building block.
    /// </summary>
    public static IServiceCollection AddVKObservabilityBlock(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return ObservabilityBlockRegistration.Register(services, configuration);
    }
}

