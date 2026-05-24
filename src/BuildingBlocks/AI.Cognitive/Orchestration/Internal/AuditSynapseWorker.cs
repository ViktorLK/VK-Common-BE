using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Cognitive.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

// // [AP.01] Sealed default for classes
internal sealed class DefaultAuditSynapseQueue : IVKAuditSynapseQueue
{
    private readonly Channel<VKAuditSynapseEvent> _queue;

    public DefaultAuditSynapseQueue()
    {
        var options = new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest // Fallback safety for extreme bursts
        };
        _queue = Channel.CreateBounded<VKAuditSynapseEvent>(options);
    }

    public async ValueTask EnqueueAsync(VKAuditSynapseEvent auditEvent, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(auditEvent);
        await _queue.Writer.WriteAsync(auditEvent, cancellationToken).ConfigureAwait(false); // [CS.03]
    }

    public async ValueTask<VKAuditSynapseEvent> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
    }
}

/// <summary>
/// A background hosted service that processes token audits out-of-band to prevent scoped service disposal issues.
/// </summary>
internal sealed class AuditSynapseWorker : BackgroundService
{
    private readonly IVKAuditSynapseQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditSynapseWorker> _logger;

    public AuditSynapseWorker(
        IVKAuditSynapseQueue queue,
        IServiceProvider serviceProvider,
        ILogger<AuditSynapseWorker> logger)
    {
        _queue = VKGuard.NotNull(queue);
        _serviceProvider = VKGuard.NotNull(serviceProvider);
        _logger = VKGuard.NotNull(logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var auditEvent = await _queue.DequeueAsync(stoppingToken).ConfigureAwait(false); // [CS.03]

                // Create a fresh scope for executing interceptors safely
                using var scope = _serviceProvider.CreateScope();
                var interceptors = scope.ServiceProvider.GetRequiredService<IEnumerable<IVKCognitivePipelineInterceptor>>();

                foreach (var interceptor in interceptors)
                {
                    try
                    {
                        var result = await interceptor.OnAfterChatAsync(auditEvent.Context, auditEvent.ChatResponse, stoppingToken).ConfigureAwait(false); // [CS.03]
                        if (result.IsFailure)
                        {
                            VKAICognitiveLog.LogInterceptorBackgroundError(
                                _logger,
                                interceptor.GetType().Name,
                                auditEvent.Context.SessionId,
                                new Exception(result.FirstError.Description));
                        }
                    }
                    catch (Exception ex)
                    {
                        VKAICognitiveLog.LogInterceptorBackgroundError(
                            _logger,
                            interceptor.GetType().Name,
                            auditEvent.Context.SessionId,
                            ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogQueueProcessingError(ex);
            }
        }
    }
}
