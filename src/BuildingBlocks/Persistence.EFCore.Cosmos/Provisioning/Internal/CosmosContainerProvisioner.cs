using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.Cosmos;
using VK.Blocks.Persistence.Cosmos.Connection;
using VK.Blocks.Persistence.Cosmos.Common.Diagnostics.Internal;

namespace VK.Blocks.Persistence.Cosmos.Provisioning.Internal;

/// <summary>
/// Dynamic container provisioner supporting Hierarchical Partition Keys, TTL, and Analytical Store.
/// </summary>
internal sealed class CosmosContainerProvisioner : IVKCosmosContainerProvisioner
{
    private readonly IVKCosmosDbConnection _dbConnection;
    private readonly CosmosIndexPolicyBuilder _indexPolicyBuilder;
    private readonly ILogger<CosmosContainerProvisioner> _logger;

    public CosmosContainerProvisioner(
        IVKCosmosDbConnection dbConnection,
        CosmosIndexPolicyBuilder indexPolicyBuilder,
        ILogger<CosmosContainerProvisioner> logger)
    {
        _dbConnection = VKGuard.NotNull(dbConnection);
        _indexPolicyBuilder = VKGuard.NotNull(indexPolicyBuilder);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ProvisionContainerAsync(
        VKCosmosContainerDefinition definition,
        CancellationToken ct)
    {
        VKGuard.NotNull(definition);

        try
        {
            var containerProperties = new ContainerProperties
            {
                Id = definition.ContainerName
            };

            // Configure partitioning strategy
            if (definition.HierarchicalPartitionKeyPaths != null && definition.HierarchicalPartitionKeyPaths.Count > 0)
            {
                containerProperties.PartitionKeyPaths = new List<string>(definition.HierarchicalPartitionKeyPaths);
            }
            else
            {
                containerProperties.PartitionKeyPath = definition.PartitionKeyPath;
            }

            // Configure TTL
            if (definition.DefaultTimeToLiveSeconds.HasValue)
            {
                containerProperties.DefaultTimeToLive = definition.DefaultTimeToLiveSeconds.Value;
            }

            // Configure Analytical Store
            if (definition.EnableAnalyticalStore)
            {
                containerProperties.AnalyticalStoreTimeToLiveInSeconds = -1; // Default to run indefinitely
            }

            // Configure Indexing Policy
            containerProperties.IndexingPolicy = _indexPolicyBuilder.BuildDefaultPolicy();

            await _dbConnection.Database.CreateContainerIfNotExistsAsync(containerProperties, cancellationToken: ct).ConfigureAwait(false);

            CosmosLog.LogContainerProvisioned(_logger, definition.ContainerName, definition.DefaultTimeToLiveSeconds, definition.EnableAnalyticalStore);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(Errors.Provisioning.ContainerCreationFailed(definition.ContainerName, ex.Message));
        }
    }
}
