using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Mapping.Options;

namespace VK.Blocks.Mapping.DependencyInjection;

/// <summary>
/// Dependency injection extensions for the mapping framework (Core).
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Adds mapping options to the service collection.
    /// Real mapping implementation (e.g., AutoMapper) should be registered separately.
    /// </summary>
    public static IServiceCollection AddMappingCore(this IServiceCollection services, Action<MappingOptions>? configure = null)
    {
        var options = new MappingOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);

        return services;
    }
}
