using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore.Cosmos.Common.Diagnostics.Internal;

namespace VK.Blocks.Persistence.EFCore.Cosmos.ChangeFeed.Internal;

/// <summary>
/// Subscribes reactively to Cosmos DB updates.
/// </summary>
internal sealed class DefaultChangeFeedConsumer<T> : IVKCosmosChangeFeedProcessor where T : class
{
    private readonly ChangeFeedProcessor _processor;
    private readonly IVKChangeFeedHandler<T> _handler;
    private readonly ILogger<DefaultChangeFeedConsumer<T>> _logger;

    public DefaultChangeFeedConsumer(
        ChangeFeedObserverFactory factory,
        IVKChangeFeedHandler<T> handler,
        ILogger<DefaultChangeFeedConsumer<T>> logger)
    {
        VKGuard.NotNull(factory);
        _handler = VKGuard.NotNull(handler);
        _logger = VKGuard.NotNull(logger);

        var typeName = typeof(T).Name;
        _processor = factory.CreateProcessor<T>(
            processorName: $"{typeName}ChangeFeedProcessor",
            sourceContainerName: typeName,
            leaseContainerName: $"{typeName}Leases",
            onChangesDelegate: HandleChangesAsync);
    }

    private async Task HandleChangesAsync(
        IReadOnlyCollection<T> changes,
        CancellationToken cancellationToken)
    {
        CosmosLog.LogChangeFeedReceived(_logger, changes.Count);
        await _handler.HandleChangesAsync(changes, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _processor.StartAsync().ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure("Persistence.Cosmos.ChangeFeedStartFailed", ex.Message));
        }
    }

    public async Task<VKResult> StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _processor.StopAsync().ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure("Persistence.Cosmos.ChangeFeedStopFailed", ex.Message));
        }
    }
}
