using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Diagnostics.Internal;

namespace VK.Blocks.Resilience.Common.Internal;

// [AP.01] sealed
internal sealed class DefaultResiliencePipeline : IVKResiliencePipeline
{
    private readonly ReadOnlyCollection<IVKResiliencePolicy> _policies;

    public string PipelineName { get; }
    public IReadOnlyList<IVKResiliencePolicy> Policies => _policies;

    public DefaultResiliencePipeline(string pipelineName, IEnumerable<IVKResiliencePolicy> policies)
    {
        PipelineName = VKGuard.NotNullOrWhiteSpace(pipelineName);
        _policies = new List<IVKResiliencePolicy>(VKGuard.NotNull(policies)).AsReadOnly();
    }

    public async Task<VKResult> ExecuteAsync(
        Func<VKResilienceContext, CancellationToken, Task<VKResult>> action,
        VKResilienceContext? context = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);
        var effectiveContext = context ?? VKResilienceContext.Create(PipelineName);

        using var activity = ResilienceDiagnostics.StartActivity($"Pipeline.{PipelineName}");
        if (activity is not null)
        {
            activity.SetTag("resilience.pipeline.name", PipelineName);
            activity.SetTag("resilience.operation.key", effectiveContext.OperationKey);
            if (!string.IsNullOrEmpty(effectiveContext.TraceId))
            {
                activity.SetTag("trace.id", effectiveContext.TraceId);
            }
        }

        Func<VKResilienceContext, CancellationToken, Task<VKResult>> pipeline = action;

        // Chain policies from innermost to outermost (first policy in list wraps outermost)
        for (int i = _policies.Count - 1; i >= 0; i--)
        {
            var policy = _policies[i];
            var next = pipeline;
            pipeline = (ctx, ct) => policy.ExecuteAsync(next, ctx, ct);
        }

        try
        {
            var result = await pipeline(effectiveContext, cancellationToken).ConfigureAwait(false);
            ResilienceDiagnostics.RecordStrategyExecution(PipelineName, result.IsSuccess);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            ResilienceDiagnostics.RecordStrategyExecution(PipelineName, false);
            return VKResult.Failure(VKResilienceErrors.CreateExecutionFailed(ex.Message));
        }
    }

    public async Task<VKResult<TResult>> ExecuteAsync<TResult>(
        Func<VKResilienceContext, CancellationToken, Task<VKResult<TResult>>> action,
        VKResilienceContext? context = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);
        var effectiveContext = context ?? VKResilienceContext.Create(PipelineName);

        using var activity = ResilienceDiagnostics.StartActivity($"Pipeline.{PipelineName}");
        if (activity is not null)
        {
            activity.SetTag("resilience.pipeline.name", PipelineName);
            activity.SetTag("resilience.operation.key", effectiveContext.OperationKey);
            if (!string.IsNullOrEmpty(effectiveContext.TraceId))
            {
                activity.SetTag("trace.id", effectiveContext.TraceId);
            }
        }

        Func<VKResilienceContext, CancellationToken, Task<VKResult<TResult>>> pipeline = action;

        // Chain policies from innermost to outermost
        for (int i = _policies.Count - 1; i >= 0; i--)
        {
            var policy = _policies[i];
            var next = pipeline;
            pipeline = (ctx, ct) => policy.ExecuteAsync(next, ctx, ct);
        }

        try
        {
            var result = await pipeline(effectiveContext, cancellationToken).ConfigureAwait(false);
            ResilienceDiagnostics.RecordStrategyExecution(PipelineName, result.IsSuccess);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            ResilienceDiagnostics.RecordStrategyExecution(PipelineName, false);
            return VKResult.Failure<TResult>(VKResilienceErrors.CreateExecutionFailed(ex.Message));
        }
    }

    public async Task<VKResult> ExecuteAsync(
        Func<CancellationToken, Task> action,
        VKResilienceContext? context = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);

        return await ExecuteAsync(
            async (_, ct) =>
            {
                await action(ct).ConfigureAwait(false);
                return VKResult.Success();
            },
            context,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult<TResult>> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        VKResilienceContext? context = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(action);

        return await ExecuteAsync(
            async (_, ct) =>
            {
                var val = await action(ct).ConfigureAwait(false);
                return VKResult.Success(val);
            },
            context,
            cancellationToken).ConfigureAwait(false);
    }
}
