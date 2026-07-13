using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.Cosmos;
using VK.Blocks.Persistence.Cosmos.Connection;

namespace VK.Blocks.Persistence.Cosmos.Failover.Internal;

/// <summary>
/// Implementation of failover management using CosmosClient metadata.
/// </summary>
internal sealed class CosmosFailoverManager : IVKCosmosFailoverManager
{
    private readonly IVKCosmosDbConnection _dbConnection;
    private readonly ILogger<CosmosFailoverManager> _logger;

    public CosmosFailoverManager(IVKCosmosDbConnection dbConnection, ILogger<CosmosFailoverManager> logger)
    {
        _dbConnection = VKGuard.NotNull(dbConnection);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<IReadOnlyList<string>>> GetAvailableRegionsAsync(CancellationToken ct)
    {
        try
        {
            var accountProperties = await _dbConnection.Client.ReadAccountAsync().ConfigureAwait(false);
            var regions = new List<string>();
            foreach (var region in accountProperties.WritableRegions)
            {
                regions.Add(region.Name);
            }
            foreach (var region in accountProperties.ReadableRegions)
            {
                if (!regions.Contains(region.Name))
                {
                    regions.Add(region.Name);
                }
            }
            return VKResult.Success<IReadOnlyList<string>>(regions);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<IReadOnlyList<string>>(Errors.Failover.ReadAccountFailed(ex.Message));
        }
    }
}
