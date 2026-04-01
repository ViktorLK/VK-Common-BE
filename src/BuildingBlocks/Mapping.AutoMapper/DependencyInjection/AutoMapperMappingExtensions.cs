using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Mapping.Abstractions;
using VK.Blocks.Mapping.AutoMapper;
using VK.Blocks.Mapping.Options;

namespace VK.Blocks.Mapping.AutoMapper.DependencyInjection;

/// <summary>
/// Dependency injection extensions for the AutoMapper provider.
/// </summary>
public static class AutoMapperMappingExtensions
{
    /// <summary>
    /// Adds AutoMapper-based mapping implementation to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAutoMapperMapping(this IServiceCollection services)
    {
        // Auto-discovery from calling assemblies
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        
        // Register the adapter
        services.AddSingleton<IMapper, AutoMapperAdapter>();
        
        return services;
    }
}
