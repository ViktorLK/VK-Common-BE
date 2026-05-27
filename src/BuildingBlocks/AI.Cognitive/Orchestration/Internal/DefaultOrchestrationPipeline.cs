using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

internal sealed class DefaultOrchestrationPipeline : IVKOrchestrationPipeline
{
    private readonly IEnumerable<IVKOrchestrationPipelineStage> _stages;
    private readonly IEnumerable<IVKCognitivePipelineInterceptor> _interceptors;
    private readonly IVKPresenceTracker? _presenceTracker;
    private readonly IVKTenantContextAccessor? _tenantAccessor;
    private readonly IVKAuditSynapseQueue? _auditQueue;

    public DefaultOrchestrationPipeline(
        IEnumerable<IVKOrchestrationPipelineStage> stages,
        IEnumerable<IVKCognitivePipelineInterceptor>? interceptors = null,
        IVKPresenceTracker? presenceTracker = null,
        IVKTenantContextAccessor? tenantAccessor = null,
        IVKAuditSynapseQueue? auditQueue = null)
    {
        _stages = VKGuard.NotNull(stages);
        _interceptors = interceptors ?? Array.Empty<IVKCognitivePipelineInterceptor>();
        _presenceTracker = presenceTracker;
        _tenantAccessor = tenantAccessor;
        _auditQueue = auditQueue;
    }

    public async Task<VKResult<VKOrchestrationResult>> ExecuteAsync(
        string input,
        VKCognitivePipelineArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(input);

        var context = InitializeContext(input, args);

        // 1. Session Preemption Concurrency Lock
        var sessionLock = VKSessionLock.GetLock(context.SessionId);
        await sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        IDisposable? tenantScope = null;

        try
        {
            // 2. Capture Environment State & Freezing Tenant context
            var freezeResult = await CaptureAndFreezeTenantAsync(context, cancellationToken).ConfigureAwait(false);
            if (freezeResult.IsFailure)
            {
                context.CriticalError = VKPipelineError.From<DefaultOrchestrationPipeline>(freezeResult.FirstError.Description);
                return BuildResult(context);
            }
            tenantScope = freezeResult.Value;

            // 3. Execute Early Governance Interceptors (Priority < -150)
            var earlyResult = await RunInterceptorsAsync(context, isEarly: true, cancellationToken).ConfigureAwait(false);
            if (earlyResult.IsFailure)
            {
                context.CriticalError = VKPipelineError.From<DefaultOrchestrationPipeline>(earlyResult.FirstError.Description);
                return BuildResult(context);
            }

            // 4. Run Cognitive Stages with Order < 600 (Reasoning, Persona, Knowledge, Memory)
            await ExecuteStagesAsync(context, orderThreshold: 600, isLessThan: true, cancellationToken).ConfigureAwait(false);
            if (context.IsFaulted)
                return BuildResult(context);

            // 5. Execute Late Context Interceptors (Priority >= -150)
            var lateResult = await RunInterceptorsAsync(context, isEarly: false, cancellationToken).ConfigureAwait(false);
            if (lateResult.IsFailure)
            {
                context.CriticalError = VKPipelineError.From<DefaultOrchestrationPipeline>(lateResult.FirstError.Description);
                return BuildResult(context);
            }

            // 6. Run Weaving & Late Stages with Order >= 600 (Weaving)
            await ExecuteStagesAsync(context, orderThreshold: 600, isLessThan: false, cancellationToken).ConfigureAwait(false);
            if (context.IsFaulted)
                return BuildResult(context);

            // 7. Fire and Forget Synapse Auditing
            QueueBackgroundAudit(context);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            context.CriticalError = VKPipelineError.From<DefaultOrchestrationPipeline>("Inference execution timed out");
        }
        catch (Exception ex)
        {
            context.CriticalError = VKPipelineError.From<DefaultOrchestrationPipeline>($"Pipeline execution faulted: {ex.Message}");
        }
        finally
        {
            try
            {
                tenantScope?.Dispose();
            }
            catch
            {
                // Safety release ignore
            }
            sessionLock.Release();
        }

        return BuildResult(context);
    }

    private static VKOrchestrationPipelineContext InitializeContext(string input, VKCognitivePipelineArgs? args)
    {
        return new VKOrchestrationPipelineContext
        {
            Input = input,
            PersonaId = args?.PersonaId ?? "default-persona",
            SessionId = (args?.Context.TryGetValue("SessionId", out var sid) == true ? sid?.ToString() : null) ?? args?.UserId ?? "default-session",
            Args = args,
            Messages = args?.ChatHistory?.ToList()
        };
    }

    private async Task<VKResult<IDisposable?>> CaptureAndFreezeTenantAsync(VKOrchestrationPipelineContext context, CancellationToken ct)
    {
        if (_presenceTracker == null)
        {
            return VKResult.Success<IDisposable?>(null);
        }

        var captureResult = await _presenceTracker.CaptureStateAsync(
            context.SessionId,
            context.Input,
            context.Args?.WorldState,
            ct).ConfigureAwait(false);

        if (captureResult.IsFailure)
        {
            return VKResult.Failure<IDisposable?>(captureResult.Errors);
        }

        context.InitialPresenceState = captureResult.Value;
        var tenantId = captureResult.Value.TenantId ?? "Default";

        IDisposable? scope = null;
        if (_tenantAccessor != null)
        {
            scope = _tenantAccessor.Freeze(tenantId);
        }

        return VKResult.Success(scope);
    }

    private async Task<VKResult> RunInterceptorsAsync(VKOrchestrationPipelineContext context, bool isEarly, CancellationToken ct)
    {
        var targetInterceptors = _interceptors
            .Where(i => isEarly ? i.Priority < -150 : i.Priority >= -150)
            .OrderBy(i => i.Priority)
            .ToList();

        foreach (var interceptor in targetInterceptors)
        {
            var result = await interceptor.OnBeforeChatAsync(context, ct).ConfigureAwait(false);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return VKResult.Success();
    }

    private async Task ExecuteStagesAsync(
        VKOrchestrationPipelineContext context,
        int orderThreshold,
        bool isLessThan,
        CancellationToken ct)
    {
        var targetStages = _stages
            .Where(s => s.IsActive && (isLessThan ? s.Order < orderThreshold : s.Order >= orderThreshold))
            .OrderBy(s => s.Order)
            .ToList();

        var chunks = new List<List<IVKOrchestrationPipelineStage>>();
        List<IVKOrchestrationPipelineStage>? currentChunk = null;

        foreach (var stage in targetStages)
        {
            if (currentChunk == null)
            {
                currentChunk = [stage];
                chunks.Add(currentChunk);
            }
            else
            {
                var prev = currentChunk.Last();
                if ((stage.ParallelGroup.HasValue && stage.ParallelGroup == prev.ParallelGroup) || stage.Order == prev.Order)
                {
                    currentChunk.Add(stage);
                }
                else
                {
                    currentChunk = [stage];
                    chunks.Add(currentChunk);
                }
            }
        }

        foreach (var chunk in chunks)
        {
            if (context.IsFaulted)
                break;

            var parallel = chunk.Where(s => s.IsParallel).ToList();
            var serial = chunk.Where(s => !s.IsParallel).ToList();

            if (parallel.Count > 0)
            {
                await Task.WhenAll(parallel.Select(s => s.ExecuteAsync(context, ct))).ConfigureAwait(false);
            }

            foreach (var stage in serial)
            {
                if (context.IsFaulted)
                    break;

                await stage.ExecuteAsync(context, ct).ConfigureAwait(false);
            }
        }
    }

    private void QueueBackgroundAudit(VKOrchestrationPipelineContext context)
    {
        if (_auditQueue is null || context.IsFaulted)
            return;

        VKChatMessage? responseMessage = null;

        if (context.Response?.Message != null)
        {
            responseMessage = context.Response.Message;
        }
        else if (context.Messages != null && context.Messages.Count > 0)
        {
            var lastMsg = context.Messages.Last();
            if (lastMsg.Role == VKChatRole.Assistant)
            {
                responseMessage = lastMsg;
            }
        }

        if (responseMessage != null)
        {
            var auditEvent = new VKAuditSynapseEvent
            {
                Context = context,
                ChatResponse = responseMessage
            };

            _ = _auditQueue.EnqueueAsync(auditEvent, CancellationToken.None).AsTask();
        }
    }

    private VKResult<VKOrchestrationResult> BuildResult(VKOrchestrationPipelineContext context)
    {
        if (context.CriticalError != null)
        {
            var vkError = VKError.Failure("Pipeline.Fault", context.CriticalError.Message);
            return VKResult.Failure<VKOrchestrationResult>(vkError);
        }

        return VKResult.Success(new VKOrchestrationResult() 
        { 
            Tapestry = context.Tapestry,
            Intent = context.IntentContext?.Intent,
            Reasoning = context.Properties.TryGetValue("Reasoning", out var r) ? r?.ToString() : null,
            Output = context.Response?.Message?.Content
        });
    }
}
