using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace VK.Blocks.Infrastructure;

/// <summary>
/// Public extensions for the Infrastructure block.
/// </summary>
public static class VKInfrastructureExtensions
{
    public static IServiceCollection AddVKInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddVKInfrastructureBlock(configuration);
        return services;
    }
}
