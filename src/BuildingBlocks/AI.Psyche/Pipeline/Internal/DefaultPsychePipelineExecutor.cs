using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly ILogger<DefaultPsychePipelineExecutor> _logger;

    public DefaultPsychePipelineExecutor(
        IEnumerable<IVKPsycheBeforePipelineStage> beforeStages,
        IEnumerable<IVKPsycheAfterPipelineStage> afterStages,
        IEnumerable<IVKPsycheMiddleware> middlewares,
        ILogger<DefaultPsychePipelineExecutor> logger)
        : base(beforeStages, afterStages, middlewares)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public override async Task<VKResult<VKPsycheResponse>> ExecuteAsync(
        VKPsycheContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        _logger.ExecutionStarted(context.Request.SessionId.Value.ToString(), context.Request.CorrelationId ?? string.Empty);
        var stopwatch = Stopwatch.StartNew();

        var result = await base.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();

        if (result.IsFailure)
        {
            _logger.ExecutionFailed(
                context.Request.CorrelationId ?? string.Empty,
                result.FirstError.Code,
                result.FirstError.Description);
            return result;
        }

        _logger.ExecutionCompleted(
            context.Request.CorrelationId ?? string.Empty,
            stopwatch.Elapsed.TotalMilliseconds);

        return result;
    }

    protected override async Task<VKResult> InvokeTerminalAsync(
        VKPsycheContext context,
        CancellationToken cancellationToken)
    {
        if (context.Response.Messages.Count == 0)
        {
            return VKResult.Failure(VKPipelineErrors.EmptyResponse);
        }

        if (context.IsWeaveOnly)
        {
            return VKResult.Success();
        }

        if (context.Services.GetService(typeof(IVKChatEngine)) is not IVKChatEngine chatEngine)
        {
            return VKResult.Failure(VKPipelineErrors.ChatEngineNotFound);
        }

        var chatArgs = context.Args<VKChatArgs>();

        var chatResult = await chatEngine.SendAsync(context.Response.Messages, chatArgs, cancellationToken).ConfigureAwait(false);
        if (chatResult.IsFailure)
        {
            return VKResult.Failure(chatResult.Errors);
        }

        context.SetState(chatResult.Value);

        context.Response.ChatResponse = chatResult.Value;
        if (chatResult.Value.Usage is not null)
        {
            context.Response.Usage = chatResult.Value.Usage;
        }

        return VKResult.Success();
    }

    protected override VKPsycheResponse BuildResponse(VKPsycheContext context)
    {
        VKGuard.NotNull(context);
        return context.Response.Build();
    }

    protected override bool CheckAborted(VKPsycheContext context) => context.IsAborted;

    protected override VKResult GetAbortResult(VKPsycheContext context) => VKResult.Failure(VKPipelineErrors.Aborted);
}
