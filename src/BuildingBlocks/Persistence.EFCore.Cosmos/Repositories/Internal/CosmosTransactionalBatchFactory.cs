using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Cosmos.Connection;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Repositories.Internal;

/// <summary>
/// Default implementation of the transactional batch factory.
/// </summary>
internal sealed class CosmosTransactionalBatchFactory : IVKCosmosTransactionalBatchFactory
{
    private readonly IVKCosmosDbConnection _dbConnection;
    private readonly ILoggerFactory _loggerFactory;

    public CosmosTransactionalBatchFactory(IVKCosmosDbConnection dbConnection, ILoggerFactory loggerFactory)
    {
        _dbConnection = VKGuard.NotNull(dbConnection);
        _loggerFactory = VKGuard.NotNull(loggerFactory);
    }

    public IVKCosmosTransactionalBatch<T> CreateBatch<T>(string partitionKey) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(partitionKey);
        var containerName = typeof(T).Name;
        var container = _dbConnection.GetContainer(containerName);
        var logger = _loggerFactory.CreateLogger(typeof(CosmosTransactionalBatchExecutor<T>));
        return new CosmosTransactionalBatchExecutor<T>(container, partitionKey, logger);
    }
}
