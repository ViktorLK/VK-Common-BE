using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Cosmos.Common.Diagnostics.Internal;

namespace VK.Blocks.Persistence.EFCore.Cosmos.Repositories.Internal;

/// <summary>
/// Executes a transactional batch of operations atomically within a single logical partition key.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
internal sealed class CosmosTransactionalBatchExecutor<T> : IVKCosmosTransactionalBatch<T> where T : class
{
    private const int MaxOperations = 100;
    private readonly Container _container;
    private readonly PartitionKey _partitionKey;
    private readonly List<Action<TransactionalBatch>> _operations = new();
    private readonly ILogger _logger;

    public CosmosTransactionalBatchExecutor(Container container, string partitionKey, ILogger logger)
    {
        _container = VKGuard.NotNull(container);
        VKGuard.NotNullOrWhiteSpace(partitionKey);
        _partitionKey = new PartitionKey(partitionKey);
        _logger = VKGuard.NotNull(logger);
    }

    public IVKCosmosTransactionalBatch<T> Create(T entity)
    {
        VKGuard.NotNull(entity);
        _operations.Add(batch => batch.CreateItem(entity));
        return this;
    }

    public IVKCosmosTransactionalBatch<T> Upsert(T entity)
    {
        VKGuard.NotNull(entity);
        _operations.Add(batch => batch.UpsertItem(entity));
        return this;
    }

    public IVKCosmosTransactionalBatch<T> Delete(string id)
    {
        VKGuard.NotNullOrWhiteSpace(id);
        _operations.Add(batch => batch.DeleteItem(id));
        return this;
    }

    public async Task<VKResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_operations.Count == 0)
        {
            return VKResult.Success();
        }

        if (_operations.Count > MaxOperations)
        {
            return VKResult.Failure(Errors.Batch.OperationLimitExceeded(_operations.Count));
        }

        try
        {
            var batch = _container.CreateTransactionalBatch(_partitionKey);
            foreach (var op in _operations)
            {
                op(batch);
            }

            using var response = await batch.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            CosmosLog.LogTransactionalBatchCompleted(_logger, _container.Id, _operations.Count, response.RequestCharge);

            if (response.IsSuccessStatusCode)
            {
                return VKResult.Success();
            }

            return VKResult.Failure(Errors.Batch.ExecutionFailed(response.ErrorMessage ?? $"Transactional batch failed with status code {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return VKResult.Failure(Errors.Batch.ExecutionFailed(ex.Message));
        }
    }
}
