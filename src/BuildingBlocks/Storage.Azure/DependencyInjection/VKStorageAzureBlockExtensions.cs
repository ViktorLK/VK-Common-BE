namespace VK.Blocks.Storage.Azure.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Storage.Azure.DependencyInjection.Internal;


/// <summary>
/// Service collection extensions for Storage Azure block.
/// </summary>
public static class VKStorageAzureBlockExtensions
{
    /// <summary>
    /// Adds the Storage Azure block to the service collection.
    /// </summary>
    public static IVKStorageAzureBuilder AddStorageAzureBlock(this IServiceCollection services, IConfiguration configuration)
        => StorageAzureBlockRegistration.Register(services, configuration);
}
