using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Outbox.Internal;

internal sealed class DefaultJobOutboxProcessor : BackgroundService
{
    private readonly IVKJobOutbox _outbox;

    public DefaultJobOutboxProcessor(IVKJobOutbox outbox)
    {
        _outbox = VKGuard.NotNull(outbox);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var entriesResult = await _outbox.GetUnprocessedAsync(100, stoppingToken).ConfigureAwait(false);
            if (entriesResult.IsSuccess && entriesResult.Value != null)
            {
                foreach (var entry in entriesResult.Value)
                {
                    if (stoppingToken.IsCancellationRequested) break;
                    await _outbox.MarkProcessedAsync(entry.Id, stoppingToken).ConfigureAwait(false);
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
