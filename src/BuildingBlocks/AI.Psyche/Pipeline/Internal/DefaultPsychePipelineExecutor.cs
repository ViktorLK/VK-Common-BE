using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Pipeline.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pipeline.Internal;

/// <summary>
/// Default implementation of the Psyche pipeline executor.
/// Inherits from <see cref="VKPipelineExecutorBase{TContext, TResponse}"/> and handles the terminal ChatEngine execution.
/// </summary>
internal sealed class DefaultPsychePipelineExecutor : VKPipelineExecutorBase<VKPsycheContext, VKPsycheResponse>, IVKPsychePipelineExecutor
{
    private sealed record StageMetadata(string RawName, string SpanName, ActivityKind Kind);
    private static readonly ConcurrentDictionary<Type, StageMetadata> StageMetadataCache = new();

    private readonly ILogger<DefaultPsychePipelineExecutor> _logger;

    public DefaultPsychePipelineExecutor(
        IEnumerable<IVKPsychePipelineStage> stages,
        IEnumerable<IVKPsycheMiddleware> middlewares,
        ILogger<DefaultPsychePipelineExecutor> logger)
        : base(stages, middlewares)
    {
        _logger = VKGuard.NotNull(logger);
    }

    private static StageMetadata GetStageMetadata(Type stageType)
    {
        return StageMetadataCache.GetOrAdd(stageType, static t =>
        {
            var traceAttr = (VKTraceAttribute?)System.Attribute.GetCustomAttribute(t, typeof(VKTraceAttribute));
            var rawName = t.Name.Replace("Default", string.Empty).Replace("Stage", string.Empty);
            var spanName = traceAttr?.ActivityName ?? $"psyche.stage.{rawName.ToLowerInvariant()}";
            return new StageMetadata(rawName, spanName, traceAttr?.Kind ?? ActivityKind.Internal);
        });
    }

    public override async Task<VKResult<VKPsycheResponse>> ExecuteAsync(
        VKPsycheContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        using var activity = PipelineDiagnostics.Source.StartActivity("psyche.pipeline.execute");
        activity?.SetTag(VKPsycheDiagnosticsConstants.Tags.GenAiSystem, "psyche");
        activity?.SetTag(VKPsycheDiagnosticsConstants.Tags.SessionId, context.Request.SessionId.Value.ToString());
        activity?.SetTag(VKPsycheDiagnosticsConstants.Tags.CorrelationId, context.CorrelationId);

        _logger.ExecutionStarted(context.Request.SessionId, context.CorrelationId);
        var stopwatch = Stopwatch.StartNew();

        var result = await base.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();
        var durationMs = stopwatch.Elapsed.TotalMilliseconds;
        PipelineDiagnostics.RecordPipelineExecution(durationMs, result.IsSuccess);

        if (result.IsFailure)
        {
            activity?.SetStatus(ActivityStatusCode.Error, result.FirstError.Description);
            activity?.SetTag(VKPsycheDiagnosticsConstants.Tags.ErrorCode, result.FirstError.Code);
            _logger.ExecutionFailed(
                context.CorrelationId,
                result.FirstError.Code,
                result.FirstError.Description);
            return result;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        _logger.ExecutionCompleted(
            context.CorrelationId,
            durationMs);

        return result;
    }

    protected override async Task<VKResult> InvokeTerminalAsync(
        VKPsycheContext context,
        CancellationToken cancellationToken)
    {
        if (context.Services.GetService<IVKChatEngine>() is not { } chatEngine)
        {
            return VKResult.Failure(VKPipelineErrors.ChatEngineNotFound);
        }

        var chatArgs = context.Args<VKChatArgs>();
        var modelMetadata = context.State<VKAIModelMetadata>();
        var modelId = modelMetadata?.ModelId ?? "unknown";

        using var activity = PipelineDiagnostics.Source.StartActivity("psyche.llm.invoke");
        activity?.SetTag(VKPsycheDiagnosticsConstants.Tags.RequestModel, modelId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var chatResult = await chatEngine.SendAsync(context.ResponseBuilder.Messages, chatArgs, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            PipelineDiagnostics.RecordLLMInvocation(stopwatch.Elapsed.TotalMilliseconds, modelId, chatResult.IsSuccess);

            if (chatResult.IsFailure)
            {
                activity?.SetStatus(ActivityStatusCode.Error, chatResult.FirstError.Description);
                activity?.SetTag(VKPsycheDiagnosticsConstants.Tags.ErrorCode, chatResult.FirstError.Code);
                return VKResult.Failure(chatResult.Errors);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            context.SetState(chatResult.Value);

            context.ResponseBuilder.ChatResponse = chatResult.Value;
            if (chatResult.Value.Usage is not null)
            {
                context.ResponseBuilder.Usage = chatResult.Value.Usage;
                activity?.SetTag(VKPsycheDiagnosticsConstants.Tags.PromptTokens, chatResult.Value.Usage.InputTokens);
                activity?.SetTag(VKPsycheDiagnosticsConstants.Tags.CompletionTokens, chatResult.Value.Usage.OutputTokens);
                activity?.SetTag(VKPsycheDiagnosticsConstants.Tags.TotalTokens, chatResult.Value.Usage.TotalTokens);
            }

            return VKResult.Success();
        }
        catch
        {
            stopwatch.Stop();
            PipelineDiagnostics.RecordLLMInvocation(stopwatch.Elapsed.TotalMilliseconds, modelId, false);
            throw;
        }
        finally
        {
            context.ResponseBuilder.ProfilingMetrics[VKPsycheProfilingKeys.LLMInvocation] = stopwatch.Elapsed.TotalMilliseconds;
        }
    }

    protected override async Task<VKResult> ExecuteComponentAsync(
        IVKPipelineComponent<VKPsycheContext> component,
        VKPsycheContext context,
        CancellationToken cancellationToken)
    {
        var metadata = GetStageMetadata(component.GetType());

        using var activity = PipelineDiagnostics.Source.StartActivity(metadata.SpanName, metadata.Kind);
        activity?.SetPsycheStage(metadata.RawName);
        activity?.SetPsycheCorrelationId(context.CorrelationId);

        var stopwatch = Stopwatch.StartNew();
        bool isSuccess = false;
        try
        {
            var result = await component.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                activity?.SetGenAiError(result.FirstError.Code, result.FirstError.Description);
            }
            else
            {
                isSuccess = true;
                activity?.SetGenAiOk();
            }
            return result;
        }
        catch (System.Exception ex)
        {
            activity?.SetGenAiError("UNHANDLED_STAGE_EXCEPTION", ex.Message);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var durationMs = stopwatch.Elapsed.TotalMilliseconds;
            var profilingKey = $"{metadata.RawName}Stage";
            context.ResponseBuilder.ProfilingMetrics[profilingKey] = durationMs;
            PipelineDiagnostics.RecordStageExecution(durationMs, metadata.RawName, isSuccess);
        }
    }

    protected override VKPsycheResponse BuildResponse(VKPsycheContext context)
    {
        VKGuard.NotNull(context);
        return context.ResponseBuilder.Build(context);
    }

    protected override bool CheckAborted(VKPsycheContext context) => context.IsAborted;

    protected override bool CheckCompleted(VKPsycheContext context) => context.IsCompleted;

    protected override VKResult GetAbortResult(VKPsycheContext context) => VKResult.Failure(VKPipelineErrors.Aborted);
}
