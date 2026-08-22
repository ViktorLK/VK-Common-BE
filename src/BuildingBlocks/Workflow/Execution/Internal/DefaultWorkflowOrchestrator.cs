using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Resilience;
using VK.Blocks.Workflow.Common.Diagnostics.Internal;
using VK.Blocks.Workflow.Compensation.Internal;

namespace VK.Blocks.Workflow.Execution.Internal;

/// <summary>
/// Default industrial orchestrator implementation for executing resilient, versioned, suspendable Workflow pipelines and sub-workflows.
/// Follows AP.01, CS.01, CS.03, CS.06, OR.03.
/// </summary>
internal sealed class DefaultWorkflowOrchestrator : IVKWorkflowOrchestrator
{
    private readonly IVKWorkflowStore _workflowStore;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly DefaultWorkflowCompensationExecutor _compensationExecutor;
    private readonly DefaultWorkflowMetrics _metrics;
    private readonly IVKWorkflowAlertHandler _alertHandler;
    private readonly IVKRetryExecutor _retryExecutor;
    private readonly IVKTimeoutExecutor _timeoutExecutor;
    private readonly IVKCircuitBreaker _circuitBreaker;
    private readonly IOptionsSnapshot<VKWorkflowOptions> _options;
    private readonly ILogger<DefaultWorkflowOrchestrator> _logger;

    public DefaultWorkflowOrchestrator(
        IVKWorkflowStore workflowStore,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IVKJsonSerializer jsonSerializer,
        DefaultWorkflowCompensationExecutor compensationExecutor,
        DefaultWorkflowMetrics metrics,
        IVKWorkflowAlertHandler alertHandler,
        IVKRetryExecutor retryExecutor,
        IVKTimeoutExecutor timeoutExecutor,
        IVKCircuitBreaker circuitBreaker,
        IOptionsSnapshot<VKWorkflowOptions> options,
        ILogger<DefaultWorkflowOrchestrator> logger)
    {
        _workflowStore = VKGuard.NotNull(workflowStore);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _compensationExecutor = VKGuard.NotNull(compensationExecutor);
        _metrics = VKGuard.NotNull(metrics);
        _alertHandler = VKGuard.NotNull(alertHandler);
        _retryExecutor = VKGuard.NotNull(retryExecutor);
        _timeoutExecutor = VKGuard.NotNull(timeoutExecutor);
        _circuitBreaker = VKGuard.NotNull(circuitBreaker);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<TResult>> ExecuteAsync<TContext, TResult>(
        string workflowName,
        string traceId,
        TContext context,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        IVKWorkflowCompensationHandler<TContext>? compensationHandler = null,
        VKStepResiliencePolicy? resiliencePolicy = null,
        int definitionVersion = 1,
        VKWorkflowId? parentWorkflowId = null,
        string? parentTraceId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(workflowName);
        VKGuard.NotNullOrWhiteSpace(traceId);
        VKGuard.NotNull(stepHandler);
        cancellationToken.ThrowIfCancellationRequested();

        var startTime = _timeProvider.GetUtcNow();
        var effectiveResiliencePolicy = resiliencePolicy ?? VKStepResiliencePolicy.Default;

        // 1. Idempotency & Prior Execution Check
        var checkResult = await CheckPriorExecutionAsync<TResult>(workflowName, traceId, cancellationToken).ConfigureAwait(false);
        if (checkResult is not null)
        {
            return checkResult;
        }

        // 2. Initialize Workflow Instance
        var now = _timeProvider.GetUtcNow();
        var timeoutSeconds = _options.Value.DefaultTimeoutThresholdSeconds;
        var instanceId = new VKWorkflowId(_guidGenerator.Create());

        var instance = new VKWorkflowInstance
        {
            Id = instanceId,
            TraceId = traceId,
            CorrelationId = correlationId,
            WorkflowName = workflowName,
            DefinitionVersion = Math.Max(1, definitionVersion),
            ParentWorkflowId = parentWorkflowId,
            ParentTraceId = parentTraceId,
            CurrentState = VKWorkflowState.Pending,
            PayloadJson = _jsonSerializer.Serialize(context),
            CreatedAt = now,
            UpdatedAt = now,
            NextTimeoutAt = now.AddSeconds(timeoutSeconds)
        };

        var createResult = await _workflowStore.CreateAsync(instance, cancellationToken).ConfigureAwait(false);
        if (createResult.IsFailure)
        {
            return VKResult.Failure<TResult>(createResult.FirstError);
        }

        _metrics.RecordInstanceCreated(workflowName);

        if (parentWorkflowId.HasValue)
        {
            _logger.SubWorkflowStarted(workflowName, traceId, parentWorkflowId.Value.ToString(), parentTraceId ?? string.Empty);
        }

        // 3. Phase 1 (Before)
        var beforePhase = await ExecuteBeforePhaseAsync(instance, context, stepHandler, timeoutSeconds, cancellationToken).ConfigureAwait(false);
        if (beforePhase.IsFailure)
        {
            RecordFailureMetrics(workflowName, beforePhase.FirstError.Code, startTime);
            return VKResult.Failure<TResult>(beforePhase.FirstError);
        }
        instance = beforePhase.Value;

        // 4. Phase 2 (External Execution with Step Resilience Policy)
        var externalResult = await ExecuteExternalPhaseWithResilienceAsync(instance, workflowName, traceId, context, stepHandler, effectiveResiliencePolicy, cancellationToken).ConfigureAwait(false);

        // 5. Phase 3 (After Success)
        if (externalResult.IsSuccess)
        {
            var successPhase = await ExecuteAfterSuccessPhaseAsync(instance, workflowName, traceId, context, externalResult.Value, stepHandler, startTime, cancellationToken).ConfigureAwait(false);
            if (successPhase.IsSuccess)
            {
                return externalResult;
            }

            externalResult = VKResult.Failure<TResult>(successPhase.FirstError);
        }

        // 6. Phase 4 (Failure & Compensation)
        await ExecuteFailurePhaseAsync(instance, workflowName, traceId, context, externalResult.FirstError, stepHandler, compensationHandler, startTime, cancellationToken).ConfigureAwait(false);

        return externalResult;
    }

    public async Task<VKResult<TResult>> ExecuteSubWorkflowAsync<TContext, TResult>(
        VKWorkflowId parentWorkflowId,
        string parentTraceId,
        string workflowName,
        string traceId,
        TContext context,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        IVKWorkflowCompensationHandler<TContext>? compensationHandler = null,
        VKStepResiliencePolicy? resiliencePolicy = null,
        int definitionVersion = 1,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(parentTraceId);
        return await ExecuteAsync(
            workflowName,
            traceId,
            context,
            stepHandler,
            compensationHandler,
            resiliencePolicy,
            definitionVersion,
            parentWorkflowId,
            parentTraceId,
            correlationId,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> SuspendAsync(
        VKWorkflowId id,
        string reason,
        DateTimeOffset? suspendTimeoutAt = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(reason);
        cancellationToken.ThrowIfCancellationRequested();

        var getResult = await _workflowStore.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (getResult.IsFailure)
        {
            return getResult;
        }

        var instance = getResult.Value;
        if (instance.CurrentState != VKWorkflowState.Processing)
        {
            return VKResult.Failure(VKWorkflowErrors.ConcurrentExecutionConflict);
        }

        var now = _timeProvider.GetUtcNow();
        var timeout = suspendTimeoutAt ?? now.AddSeconds(_options.Value.DefaultTimeoutThresholdSeconds);

        var suspendedInstance = instance with
        {
            CurrentState = VKWorkflowState.Suspended,
            SuspendReason = reason,
            UpdatedAt = now,
            NextTimeoutAt = timeout
        };

        var updateResult = await _workflowStore.UpdateAsync(suspendedInstance, VKWorkflowState.Processing, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsSuccess)
        {
            _logger.WorkflowSuspended(id.ToString(), reason, timeout);
            await AppendHistoryAsync(id, instance.TraceId, VKWorkflowState.Processing, VKWorkflowState.Suspended, "Orchestrator.Suspend", reason, cancellationToken).ConfigureAwait(false);
        }

        return updateResult;
    }

    public async Task<VKResult<TResult>> ResumeAsync<TContext, TResult, TResumePayload>(
        VKWorkflowId id,
        TResumePayload resumePayload,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        IVKWorkflowCompensationHandler<TContext>? compensationHandler = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(stepHandler);
        cancellationToken.ThrowIfCancellationRequested();

        var startTime = _timeProvider.GetUtcNow();
        var getResult = await _workflowStore.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (getResult.IsFailure)
        {
            return VKResult.Failure<TResult>(getResult.FirstError);
        }

        var instance = getResult.Value;
        if (instance.CurrentState != VKWorkflowState.Suspended)
        {
            return VKResult.Failure<TResult>(VKWorkflowErrors.ConcurrentExecutionConflict);
        }

        if (string.IsNullOrWhiteSpace(instance.PayloadJson))
        {
            return VKResult.Failure<TResult>(VKError.Validation("Workflow.InvalidPayload", "Cannot resume workflow with empty payload context."));
        }

        var context = _jsonSerializer.Deserialize<TContext>(instance.PayloadJson);
        if (context is null)
        {
            return VKResult.Failure<TResult>(VKError.Validation("Workflow.InvalidPayload", "Failed to deserialize workflow context."));
        }

        // 1. If stepHandler supports IVKWorkflowResumeHandler, invoke OnResumeAsync to merge payload
        if (stepHandler is IVKWorkflowResumeHandler<TContext, TResumePayload> resumeHandler)
        {
            var resumeResult = await resumeHandler.OnResumeAsync(context, resumePayload, cancellationToken).ConfigureAwait(false);
            if (resumeResult.IsFailure)
            {
                return VKResult.Failure<TResult>(resumeResult.FirstError);
            }
        }

        // 2. CAS transition: Suspended -> Processing
        var now = _timeProvider.GetUtcNow();
        var processingInstance = instance with
        {
            CurrentState = VKWorkflowState.Processing,
            PayloadJson = _jsonSerializer.Serialize(context),
            SuspendReason = null,
            UpdatedAt = now,
            NextTimeoutAt = now.AddSeconds(_options.Value.DefaultTimeoutThresholdSeconds)
        };

        var transitionResult = await _workflowStore.UpdateAsync(processingInstance, VKWorkflowState.Suspended, cancellationToken).ConfigureAwait(false);
        if (transitionResult.IsFailure)
        {
            return VKResult.Failure<TResult>(transitionResult.FirstError);
        }

        _logger.WorkflowResumed(id.ToString());
        await AppendHistoryAsync(id, instance.TraceId, VKWorkflowState.Suspended, VKWorkflowState.Processing, "Orchestrator.Resume", null, cancellationToken).ConfigureAwait(false);

        processingInstance = processingInstance with { Version = processingInstance.Version + 1 };

        // 3. Continue execution from External Phase
        var externalResult = await ExecuteExternalPhaseWithResilienceAsync(processingInstance, instance.WorkflowName, instance.TraceId, context, stepHandler, VKStepResiliencePolicy.Default, cancellationToken).ConfigureAwait(false);

        // 4. Phase 3 (After Success)
        if (externalResult.IsSuccess)
        {
            var successPhase = await ExecuteAfterSuccessPhaseAsync(processingInstance, instance.WorkflowName, instance.TraceId, context, externalResult.Value, stepHandler, startTime, cancellationToken).ConfigureAwait(false);
            if (successPhase.IsSuccess)
            {
                return externalResult;
            }

            externalResult = VKResult.Failure<TResult>(successPhase.FirstError);
        }

        // 5. Phase 4 (Failure & Compensation)
        await ExecuteFailurePhaseAsync(processingInstance, instance.WorkflowName, instance.TraceId, context, externalResult.FirstError, stepHandler, compensationHandler, startTime, cancellationToken).ConfigureAwait(false);

        return externalResult;
    }

    private async Task<VKResult<TResult>?> CheckPriorExecutionAsync<TResult>(
        string workflowName,
        string traceId,
        CancellationToken cancellationToken)
    {
        var existingResult = await _workflowStore.GetByTraceIdAsync(traceId, workflowName, cancellationToken).ConfigureAwait(false);
        if (existingResult.IsFailure)
        {
            return null;
        }

        var existing = existingResult.Value;
        if (existing.CurrentState == VKWorkflowState.Completed && !string.IsNullOrWhiteSpace(existing.ResultJson))
        {
            var cachedResult = _jsonSerializer.Deserialize<TResult>(existing.ResultJson);
            if (cachedResult is not null)
            {
                _logger.WorkflowShortCircuited(workflowName, traceId);
                return VKResult.Success(cachedResult);
            }
        }

        if (existing.CurrentState is VKWorkflowState.Processing or VKWorkflowState.Compensating or VKWorkflowState.Suspended)
        {
            _logger.WorkflowAlreadyInProgress(workflowName, traceId);
            return VKResult.Failure<TResult>(VKWorkflowErrors.DuplicateTraceId);
        }

        return null;
    }

    private async Task<VKResult<VKWorkflowInstance>> ExecuteBeforePhaseAsync<TContext, TResult>(
        VKWorkflowInstance instance,
        TContext context,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        var beforeResult = await stepHandler.OnBeforeAsync(context, cancellationToken).ConfigureAwait(false);
        if (beforeResult.IsFailure)
        {
            var now = _timeProvider.GetUtcNow();
            var failedInstance = instance with
            {
                CurrentState = VKWorkflowState.Failed,
                LastError = beforeResult.FirstError.Description,
                UpdatedAt = now
            };
            await _workflowStore.UpdateAsync(failedInstance, VKWorkflowState.Pending, cancellationToken).ConfigureAwait(false);
            await AppendHistoryAsync(instance.Id, instance.TraceId, VKWorkflowState.Pending, VKWorkflowState.Failed, "Orchestrator.BeforeFailed", beforeResult.FirstError.Description, cancellationToken).ConfigureAwait(false);
            return VKResult.Failure<VKWorkflowInstance>(beforeResult.FirstError);
        }

        var transitionNow = _timeProvider.GetUtcNow();
        var processingInstance = instance with
        {
            CurrentState = VKWorkflowState.Processing,
            UpdatedAt = transitionNow,
            NextTimeoutAt = transitionNow.AddSeconds(timeoutSeconds)
        };

        var toProcessingResult = await _workflowStore.UpdateAsync(processingInstance, VKWorkflowState.Pending, cancellationToken).ConfigureAwait(false);
        if (toProcessingResult.IsFailure)
        {
            return VKResult.Failure<VKWorkflowInstance>(toProcessingResult.FirstError);
        }

        await AppendHistoryAsync(instance.Id, instance.TraceId, VKWorkflowState.Pending, VKWorkflowState.Processing, "Orchestrator.BeforeSuccess", null, cancellationToken).ConfigureAwait(false);

        return VKResult.Success(processingInstance with { Version = processingInstance.Version + 1 });
    }

    private async Task<VKResult<TResult>> ExecuteExternalPhaseWithResilienceAsync<TContext, TResult>(
        VKWorkflowInstance instance,
        string workflowName,
        string traceId,
        TContext context,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        VKStepResiliencePolicy resiliencePolicy,
        CancellationToken cancellationToken)
    {
        var cbPolicy = resiliencePolicy.CircuitBreaker;
        var cbKey = cbPolicy?.CircuitBreakerKey;

        // 1. Circuit Breaker Pre-Check (Fast-Fail if Circuit is OPEN)
        if (!string.IsNullOrEmpty(cbKey) && !_circuitBreaker.IsAllowed(cbKey))
        {
            _logger.StepCircuitBreakerOpen(workflowName, traceId, cbKey);
            return VKResult.Failure<TResult>(VKWorkflowErrors.CircuitBreakerOpen);
        }

        var retryPolicy = resiliencePolicy.Retry ?? VKStepRetryPolicy.None;
        var timeoutPolicy = resiliencePolicy.Timeout;
        var maxAttempts = Math.Max(0, retryPolicy.MaxRetries) + 1;
        var callStartTime = Stopwatch.GetTimestamp();

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            VKResult<TResult> result;
            try
            {
                if (timeoutPolicy is not null)
                {
                    var timeoutDuration = timeoutPolicy.Timeout;
                    var timeoutResult = await _timeoutExecutor.ExecuteWithTimeoutAsync(
                        async stepCt => await stepHandler.ExecuteExternalAsync(context, stepCt).ConfigureAwait(false),
                        timeout: timeoutDuration,
                        cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (timeoutResult.IsFailure)
                    {
                        if (timeoutResult.FirstError.Code.Contains("Timeout", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.StepTimeoutExceeded(workflowName, traceId, timeoutDuration.TotalSeconds);
                            result = VKResult.Failure<TResult>(VKWorkflowErrors.StepTimeout);
                        }
                        else
                        {
                            result = VKResult.Failure<TResult>(timeoutResult.FirstError);
                        }
                    }
                    else
                    {
                        result = timeoutResult.Value;
                    }
                }
                else
                {
                    result = await stepHandler.ExecuteExternalAsync(context, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.WorkflowExternalExecutionException(workflowName, traceId, ex);
                result = VKResult.Failure<TResult>(VKWorkflowErrors.ExternalExecutionFailed);
            }

            if (result.IsSuccess)
            {
                if (!string.IsNullOrEmpty(cbKey))
                {
                    _circuitBreaker.RecordSuccess(cbKey);
                }

                var duration = Stopwatch.GetElapsedTime(callStartTime).TotalSeconds;
                _metrics.RecordExternalCallDuration(workflowName, duration, success: true);
                return result;
            }

            var error = result.FirstError;

            // Record failure to Circuit Breaker if configured
            if (!string.IsNullOrEmpty(cbKey))
            {
                _circuitBreaker.RecordFailure(cbKey, new InvalidOperationException(error.Description));
            }

            var isTransient = retryPolicy.IsTransient(error);
            if (!isTransient)
            {
                _logger.NonTransientErrorEncountered(workflowName, traceId, error.Description);
                var duration = Stopwatch.GetElapsedTime(callStartTime).TotalSeconds;
                _metrics.RecordExternalCallDuration(workflowName, duration, success: false);
                return result;
            }

            if (attempt < maxAttempts)
            {
                var delay = retryPolicy.CalculateDelay(attempt);
                if (retryPolicy.UseJitter)
                {
                    // Apply +/- 20% jitter
                    var jitter = (Random.Shared.NextDouble() * 0.4) + 0.8;
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * jitter);
                }

                _logger.StepRetryScheduled(workflowName, traceId, attempt, retryPolicy.MaxRetries, error.Description, delay.TotalMilliseconds);
                await Task.Delay(delay, _timeProvider, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var duration = Stopwatch.GetElapsedTime(callStartTime).TotalSeconds;
                _metrics.RecordExternalCallDuration(workflowName, duration, success: false);
                return result;
            }
        }

        return VKResult.Failure<TResult>(VKWorkflowErrors.ExternalExecutionFailed);
    }

    private async Task<VKResult> ExecuteAfterSuccessPhaseAsync<TContext, TResult>(
        VKWorkflowInstance instance,
        string workflowName,
        string traceId,
        TContext context,
        TResult result,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        DateTimeOffset startTime,
        CancellationToken cancellationToken)
    {
        var afterSuccessResult = await stepHandler.OnAfterSuccessAsync(context, result, cancellationToken).ConfigureAwait(false);
        if (afterSuccessResult.IsSuccess)
        {
            var now = _timeProvider.GetUtcNow();
            var completedInstance = instance with
            {
                CurrentState = VKWorkflowState.Completed,
                ResultJson = _jsonSerializer.Serialize(result),
                UpdatedAt = now
            };

            var updateResult = await _workflowStore.UpdateAsync(completedInstance, VKWorkflowState.Processing, cancellationToken).ConfigureAwait(false);
            if (updateResult.IsSuccess)
            {
                await AppendHistoryAsync(instance.Id, instance.TraceId, VKWorkflowState.Processing, VKWorkflowState.Completed, "Orchestrator.AfterSuccess", null, cancellationToken).ConfigureAwait(false);
                var e2eDuration = (now - startTime).TotalSeconds;
                _metrics.RecordInstanceCompleted(workflowName, e2eDuration);
                return VKResult.Success();
            }

            return updateResult;
        }

        _logger.WorkflowAfterSuccessFailed(workflowName, traceId, afterSuccessResult.FirstError.Description);
        return VKResult.Failure(afterSuccessResult.FirstError);
    }

    private async Task ExecuteFailurePhaseAsync<TContext, TResult>(
        VKWorkflowInstance instance,
        string workflowName,
        string traceId,
        TContext context,
        VKError failureError,
        IVKWorkflowStepHandler<TContext, TResult> stepHandler,
        IVKWorkflowCompensationHandler<TContext>? compensationHandler,
        DateTimeOffset startTime,
        CancellationToken cancellationToken)
    {
        var afterFailureResult = await stepHandler.OnAfterFailureAsync(context, failureError, cancellationToken).ConfigureAwait(false);
        if (afterFailureResult.IsFailure)
        {
            _logger.WorkflowAfterFailureFailed(workflowName, traceId, afterFailureResult.FirstError.Description);
        }

        if (compensationHandler is not null)
        {
            var now = _timeProvider.GetUtcNow();
            var compensatingInstance = instance with
            {
                CurrentState = VKWorkflowState.Compensating,
                UpdatedAt = now
            };
            var toCompResult = await _workflowStore.UpdateAsync(compensatingInstance, VKWorkflowState.Processing, cancellationToken).ConfigureAwait(false);
            if (toCompResult.IsSuccess)
            {
                compensatingInstance = compensatingInstance with { Version = compensatingInstance.Version + 1 };
                await AppendHistoryAsync(instance.Id, instance.TraceId, VKWorkflowState.Processing, VKWorkflowState.Compensating, "Orchestrator.CompensationTriggered", failureError.Description, cancellationToken).ConfigureAwait(false);
            }

            _metrics.RecordCompensationTriggered(workflowName);

            var compResult = await _compensationExecutor.ExecuteWithRetryAsync(
                compensationHandler,
                context,
                failureError,
                cancellationToken).ConfigureAwait(false);

            var finishNow = _timeProvider.GetUtcNow();
            if (compResult.IsSuccess)
            {
                var failedInstance = compensatingInstance with
                {
                    CurrentState = VKWorkflowState.Failed,
                    LastError = failureError.Description,
                    UpdatedAt = finishNow
                };
                await _workflowStore.UpdateAsync(failedInstance, VKWorkflowState.Compensating, cancellationToken).ConfigureAwait(false);
                await AppendHistoryAsync(instance.Id, instance.TraceId, VKWorkflowState.Compensating, VKWorkflowState.Failed, "Orchestrator.CompensationSuccess", failureError.Description, cancellationToken).ConfigureAwait(false);
                RecordFailureMetrics(workflowName, failureError.Code, startTime);
            }
            else
            {
                var compFailedInstance = compensatingInstance with
                {
                    CurrentState = VKWorkflowState.CompensationFailed,
                    LastError = $"Original: {failureError.Description}; Compensation: {compResult.FirstError.Description}",
                    UpdatedAt = finishNow
                };
                await _workflowStore.UpdateAsync(compFailedInstance, VKWorkflowState.Compensating, cancellationToken).ConfigureAwait(false);
                await AppendHistoryAsync(instance.Id, instance.TraceId, VKWorkflowState.Compensating, VKWorkflowState.CompensationFailed, "Orchestrator.CompensationFailed", compFailedInstance.LastError, cancellationToken).ConfigureAwait(false);
                
                _logger.UnrecoverableCompensationFailed(workflowName, traceId, compFailedInstance.LastError);
                RecordFailureMetrics(workflowName, "CompensationFailed", startTime);

                // Dispatch alert
                await _alertHandler.OnCompensationFailedAsync(compFailedInstance, failureError, compResult.FirstError, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            var now = _timeProvider.GetUtcNow();
            var failedInstance = instance with
            {
                CurrentState = VKWorkflowState.Failed,
                LastError = failureError.Description,
                UpdatedAt = now
            };
            await _workflowStore.UpdateAsync(failedInstance, VKWorkflowState.Processing, cancellationToken).ConfigureAwait(false);
            await AppendHistoryAsync(instance.Id, instance.TraceId, VKWorkflowState.Processing, VKWorkflowState.Failed, "Orchestrator.NoCompensation", failureError.Description, cancellationToken).ConfigureAwait(false);
            RecordFailureMetrics(workflowName, failureError.Code, startTime);
        }
    }

    private async Task AppendHistoryAsync(
        VKWorkflowId workflowId,
        string traceId,
        VKWorkflowState fromState,
        VKWorkflowState toState,
        string trigger,
        string? errorDescription,
        CancellationToken cancellationToken)
    {
        var entry = new VKWorkflowHistoryEntry
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            TraceId = traceId,
            FromState = fromState,
            ToState = toState,
            Trigger = trigger,
            ErrorDescription = errorDescription,
            Timestamp = _timeProvider.GetUtcNow()
        };

        await _workflowStore.AppendHistoryAsync(entry, cancellationToken).ConfigureAwait(false);
    }

    private void RecordFailureMetrics(string workflowName, string errorType, DateTimeOffset startTime)
    {
        var e2eDuration = (_timeProvider.GetUtcNow() - startTime).TotalSeconds;
        _metrics.RecordInstanceFailed(workflowName, errorType, e2eDuration);
    }
}
