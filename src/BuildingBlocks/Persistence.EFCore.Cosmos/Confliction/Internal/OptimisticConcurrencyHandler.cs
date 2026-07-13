using System;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.Cosmos;
using VK.Blocks.Persistence.Cosmos.Common.Diagnostics.Internal;

namespace VK.Blocks.Persistence.Cosmos.Confliction.Internal;

/// <summary>
/// Maps Cosmos DB HTTP status 412 concurrency conflicts into typed business Result failures.
/// </summary>
internal sealed class OptimisticConcurrencyHandler
{
    private readonly ILogger<OptimisticConcurrencyHandler> _logger;

    public OptimisticConcurrencyHandler(ILogger<OptimisticConcurrencyHandler> logger)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public VKResult HandleConcurrencyConflict(string id, string containerName, Exception exception)
    {
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNullOrWhiteSpace(containerName);
        VKGuard.NotNull(exception);

        if (exception is CosmosException cosEx && cosEx.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            CosmosLog.LogConcurrencyWarning(_logger, containerName, id, cosEx.Message);
            return VKResult.Failure(VK.Blocks.Persistence.VKPersistenceErrors.UnitOfWork.ConcurrentUpdate);
        }

        return VKResult.Failure(VKError.Failure("Persistence.Cosmos.UnexpectedConflict", exception.Message));
    }
}
