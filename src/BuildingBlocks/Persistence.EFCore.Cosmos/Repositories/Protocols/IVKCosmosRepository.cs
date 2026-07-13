using Microsoft.Azure.Cosmos;

namespace VK.Blocks.Persistence.Cosmos;

/// <summary>
/// Exposes the native Azure Cosmos SDK client and container along with the standard repository methods.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IVKCosmosRepository<TEntity> : IVKBaseRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the native Cosmos container for the entity.
    /// </summary>
    Container GetNativeContainer();

    /// <summary>
    /// Gets the native Cosmos client.
    /// </summary>
    CosmosClient GetNativeClient();
}
