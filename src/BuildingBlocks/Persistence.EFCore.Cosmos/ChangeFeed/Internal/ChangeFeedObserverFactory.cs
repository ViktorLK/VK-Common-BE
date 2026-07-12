using System;
using Microsoft.Azure.Cosmos;
using VK.Blocks.Core;
using VK.Blocks.Persistence.Cosmos.Connection;

namespace VK.Blocks.Persistence.Cosmos.ChangeFeed.Internal;

/// <summary>
/// Physical factory wrapping Cosmos lease container and processor builder.
/// </summary>
internal sealed class ChangeFeedObserverFactory
{
    private readonly IVKCosmosDbConnection _dbConnection;

    public ChangeFeedObserverFactory(IVKCosmosDbConnection dbConnection)
    {
        _dbConnection = VKGuard.NotNull(dbConnection);
    }

    public ChangeFeedProcessor CreateProcessor<T>(
        string processorName,
        string sourceContainerName,
        string leaseContainerName,
        Container.ChangesHandler<T> onChangesDelegate)
    {
        VKGuard.NotNullOrWhiteSpace(processorName);
        VKGuard.NotNullOrWhiteSpace(sourceContainerName);
        VKGuard.NotNullOrWhiteSpace(leaseContainerName);
        VKGuard.NotNull(onChangesDelegate);

        var sourceContainer = _dbConnection.GetContainer(sourceContainerName);
        var leaseContainer = _dbConnection.GetContainer(leaseContainerName);

        return sourceContainer.GetChangeFeedProcessorBuilder<T>(processorName, onChangesDelegate)
            .WithInstanceName(Environment.MachineName)
            .WithLeaseContainer(leaseContainer)
            .Build();
    }
}
