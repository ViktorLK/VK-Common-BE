using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.Cosmos.Connection;
using VK.Blocks.Persistence.Cosmos;
using VK.Blocks.Persistence.Cosmos.Common.Diagnostics.Internal;

namespace VK.Blocks.Persistence.Cosmos.Query.Internal;

/// <summary>
/// Implementation of query operations using Cosmos SDK Container.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
internal sealed class CosmosQueryRepository<T> : IVKCosmosQueryRepository<T> where T : class
{
    private readonly Container _container;
    private readonly ILogger<CosmosQueryRepository<T>> _logger;

    public CosmosQueryRepository(IVKCosmosDbConnection dbConnection, ILogger<CosmosQueryRepository<T>> logger)
    {
        VKGuard.NotNull(dbConnection);
        _logger = VKGuard.NotNull(logger);
        _container = dbConnection.GetContainer(typeof(T).Name);
    }

    public async Task<VKResult<IReadOnlyList<T>>> QueryAsync(
        QueryDefinition query,
        VKCosmosQueryOptions? queryOptions,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(query);

        try
        {
            var requestOptions = new QueryRequestOptions();

            if (queryOptions != null)
            {
                if (!string.IsNullOrWhiteSpace(queryOptions.PartitionKey))
                {
                    requestOptions.PartitionKey = new PartitionKey(queryOptions.PartitionKey);
                }
                else if (!queryOptions.EnableCrossPartitionQuery)
                {
                    // Log warning if cross-partition query is not explicitly permitted and partition key is missing.
                    CosmosLog.LogCrossPartitionQuery(_logger, _container.Id, query.QueryText);
                }

                if (queryOptions.MaxItemCount.HasValue)
                {
                    requestOptions.MaxItemCount = queryOptions.MaxItemCount.Value;
                }
            }
            else
            {
                // Warn about cross partition query if no options provided.
                CosmosLog.LogCrossPartitionQuery(_logger, _container.Id, query.QueryText);
            }

            using var iterator = _container.GetItemQueryIterator<T>(query, requestOptions: requestOptions);
            var results = new List<T>();
            double totalRequestCharge = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
                results.AddRange(response);
                totalRequestCharge += response.RequestCharge;
            }

            CosmosLog.LogQueryCompleted(_logger, _container.Id, results.Count, totalRequestCharge);
            return VKResult.Success<IReadOnlyList<T>>(results);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<IReadOnlyList<T>>(Errors.Query.ExecutionFailed(ex.Message));
        }
    }

    public async Task<VKResult<IReadOnlyList<T>>> QueryAsync(
        string sql,
        string? partitionKey,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNullOrWhiteSpace(sql);

        var queryDefinition = new QueryDefinition(sql);
        var options = new VKCosmosQueryOptions
        {
            PartitionKey = partitionKey,
            EnableCrossPartitionQuery = !string.IsNullOrWhiteSpace(partitionKey)
        };

        return await QueryAsync(queryDefinition, options, cancellationToken).ConfigureAwait(false);
    }
}
