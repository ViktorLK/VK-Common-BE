namespace VK.Blocks.Storage.Azure.DependencyInjection.Internal;

using Microsoft.Extensions.DependencyInjection;


/// <summary>
/// Implementation of the Storage Azure builder.
/// </summary>
internal sealed class StorageAzureBlockBuilder(IServiceCollection services) : IVKStorageAzureBuilder
{
    public IServiceCollection Services { get; } = services;
}
