using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace VK.Blocks.Infrastructure.Azure;

/// <summary>
/// Public extensions for the Azure Infrastructure block.
/// </summary>
public static class VKInfrastructureAzureExtensions
{
    /// <summary>
    /// Adds the Azure Infrastructure block to the service collection.
    /// </summary>
    public static IServiceCollection AddVKAzureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddVKInfrastructureAzureBlock(configuration);
        return services;
    }
}
