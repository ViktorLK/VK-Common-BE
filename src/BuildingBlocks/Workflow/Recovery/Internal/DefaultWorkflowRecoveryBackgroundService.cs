using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Workflow.Common.Diagnostics.Internal;

namespace VK.Blocks.Workflow.Recovery.Internal;

/// <summary>
/// Background sweeper service for detecting and recovering orphan in-flight Workflow instances.
/// Follows CS.03, CS.06, AP.01.
/// </summary>
internal sealed class DefaultWorkflowRecoveryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly WorkflowOrphanScanJobQueue _queue;
    private readonly IOptionsMonitor<VKWorkflowOptions> _options;
    private readonly TimeProvider _timeProvider;
    private readonly DefaultWorkflowMetrics _metrics;
    private readonly ILogger<DefaultWorkflowRecoveryBackgroundService> _logger;

    public DefaultWorkflowRecoveryBackgroundService(
        IServiceProvider serviceProvider,
        WorkflowOrphanScanJobQueue queue,
        IOptionsMonitor<VKWorkflowOptions> options,
        TimeProvider timeProvider,
        DefaultWorkflowMetrics metrics,
        ILogger<DefaultWorkflowRecoveryBackgroundService> logger)
    {
        _serviceProvider = VKGuard.NotNull(serviceProvider);
        _queue = VKGuard.NotNull(queue);
        _options = VKGuard.NotNull(options);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _metrics = VKGuard.NotNull(metrics);
        _logger = VKGuard.NotNull(logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.RecoveryServiceStarted();

        var scanTask = RunScanLoopAsync(stoppingToken);
        var processTask = RunProcessLoopAsync(stoppingToken);

        await Task.WhenAll(scanTask, processTask).ConfigureAwait(false);
    }

    private async Task RunScanLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var interval = Math.Max(5, _options.CurrentValue.OrphanScanIntervalSeconds);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(interval), _timeProvider, stoppingToken).ConfigureAwait(false);

                using var scope = _serviceProvider.CreateScope();
                var workflowStore = scope.ServiceProvider.GetRequiredService<IVKWorkflowStore>();
                var now = _timeProvider.GetUtcNow();

                var orphansResult = await workflowStore.GetOrphansAsync(now, limit: 50, stoppingToken).ConfigureAwait(false);
                if (orphansResult.IsSuccess && orphansResult.Value.Count > 0)
                {
                    _logger.OrphanWorkflowsFound(orphansResult.Value.Count);
                    _metrics.RecordOrphanDetected("All", orphansResult.Value.Count);

                    foreach (var orphan in orphansResult.Value)
                    {
                        await _queue.EnqueueAsync(orphan, stoppingToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.RecoveryScanLoopException(ex);
            }
        }
    }

    private async Task RunProcessLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var instance = await _queue.DequeueAsync(stoppingToken).ConfigureAwait(false);

                using var scope = _serviceProvider.CreateScope();
                var workflowStore = scope.ServiceProvider.GetRequiredService<IVKWorkflowStore>();
                var alertHandler = scope.ServiceProvider.GetService<IVKWorkflowAlertHandler>()
                    ?? new Observability.Internal.DefaultNoOpWorkflowAlertHandler();
                var checkers = scope.ServiceProvider.GetServices<IVKExternalCallStatusChecker>();

                var matchingChecker = checkers.FirstOrDefault(c => c.CanHandle(instance.WorkflowName));
                if (matchingChecker is not null)
                {
                    var statusResult = await matchingChecker.CheckStatusAsync(
                        instance.WorkflowName,
                        instance.TraceId,
                        instance.PayloadJson,
                        stoppingToken).ConfigureAwait(false);

                    if (statusResult.IsSuccess && statusResult.Value == VKExternalCallStatus.InProgress)
                    {
                        // Remote is still in-progress: extend timeout window
                        var extendedInstance = instance with
                        {
                            NextTimeoutAt = _timeProvider.GetUtcNow().AddSeconds(_options.CurrentValue.DefaultTimeoutThresholdSeconds),
                            UpdatedAt = _timeProvider.GetUtcNow()
                        };
                        await workflowStore.UpdateAsync(extendedInstance, instance.CurrentState, stoppingToken).ConfigureAwait(false);
                        continue;
                    }
                }

                // Transition orphan to TimeoutFailed and update timestamp
                var previousState = instance.CurrentState;
                var now = _timeProvider.GetUtcNow();
                var timeoutInstance = instance with
                {
                    CurrentState = VKWorkflowState.TimeoutFailed,
                    LastError = "Workflow execution timed out and was collected by the background recovery sweeper.",
                    UpdatedAt = now
                };

                var updateResult = await workflowStore.UpdateAsync(timeoutInstance, previousState, stoppingToken).ConfigureAwait(false);
                if (updateResult.IsSuccess)
                {
                    _logger.OrphanWorkflowMarkedTimeout(instance.WorkflowName, instance.TraceId);

                    // Append history log
                    await workflowStore.AppendHistoryAsync(new VKWorkflowHistoryEntry
                    {
                        Id = Guid.NewGuid(),
                        WorkflowId = instance.Id,
                        TraceId = instance.TraceId,
                        FromState = previousState,
                        ToState = VKWorkflowState.TimeoutFailed,
                        Trigger = "RecoverySweeper.Timeout",
                        ErrorDescription = timeoutInstance.LastError,
                        Timestamp = now
                    }, stoppingToken).ConfigureAwait(false);

                    // Dispatch alert hook
                    await alertHandler.OnWorkflowOrphanTimeoutAsync(timeoutInstance, stoppingToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.RecoveryProcessLoopException(ex);
            }
        }
    }
}
