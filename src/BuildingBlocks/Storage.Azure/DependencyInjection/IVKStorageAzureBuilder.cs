namespace VK.Blocks.Storage.Azure.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;


/// <summary>
/// A builder for configuring the Storage Azure block.
/// </summary>
public interface IVKStorageAzureBuilder
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    IServiceCollection Services { get; }
}
