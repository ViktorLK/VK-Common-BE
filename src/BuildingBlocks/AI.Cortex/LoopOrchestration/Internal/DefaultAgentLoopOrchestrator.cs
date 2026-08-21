using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Workflow;

namespace VK.Blocks.AI.Cortex.LoopOrchestration.Internal;

/// <summary>
/// Default industrial implementation of <see cref="IVKAgentLoopOrchestrator"/>.
/// Drives multi-turn Agentic loops by delegating each iteration step to <see cref="IVKChatTurnOrchestrator"/>.
/// Follows [AP.01], [CS.01], [CS.03], [CS.07], [OR.01].
/// </summary>
internal sealed class DefaultAgentLoopOrchestrator : IVKAgentLoopOrchestrator
{
    private readonly IVKChatTurnOrchestrator _turnOrchestrator;
    private readonly IOptionsSnapshot<VKLoopOrchestrationOptions> _options;
    private readonly ILogger<DefaultAgentLoopOrchestrator> _logger;
    private readonly IVKWorkflowOrchestrator? _workflowOrchestrator;

    public DefaultAgentLoopOrchestrator(
        IVKChatTurnOrchestrator turnOrchestrator,
        IOptionsSnapshot<VKLoopOrchestrationOptions> options,
        ILogger<DefaultAgentLoopOrchestrator> logger,
        IVKWorkflowOrchestrator? workflowOrchestrator = null)
    {
        _turnOrchestrator = VKGuard.NotNull(turnOrchestrator);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
        _workflowOrchestrator = workflowOrchestrator;
    }

    /// <inheritdoc />
    public async Task<VKResult<VKAgentLoopResult>> RunLoopAsync(
        VKAgentLoopRequest request,
        Func<VKChatTurnResult, bool>? exitCondition = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var maxIterations = request.MaxIterations ?? _options.Value.DefaultMaxIterations;
        var steps = new List<VKAgentIterationStep>();
        var currentRequest = request.InitialRequest;
        long totalTokens = 0;
        string finalContent = string.Empty;
        var reachedMax = false;

        for (var stepIndex = 1; stepIndex <= maxIterations; stepIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var turnResult = await _turnOrchestrator.ProcessTurnAsync(currentRequest, cancellationToken).ConfigureAwait(false);
            if (turnResult.IsFailure)
            {
                return VKResult.Failure<VKAgentLoopResult>(turnResult.FirstError);
            }

            var turn = turnResult.Value;
            totalTokens += turn.TokensUsed;
            finalContent = turn.Content;

            steps.Add(new VKAgentIterationStep
            {
                StepIndex = stepIndex,
                TurnResult = turn,
                ExecutedAt = DateTimeOffset.UtcNow
            });

            // Evaluate custom exit condition (if provided) or exit if default single-turn completed
            if (exitCondition is not null)
            {
                if (exitCondition(turn))
                {
                    break;
                }
            }
            else
            {
                // Default minimal behavior: single step completion
                break;
            }

            if (stepIndex == maxIterations)
            {
                reachedMax = true;
            }
        }

        return VKResult.Success(new VKAgentLoopResult
        {
            FinalContent = finalContent,
            TotalIterations = steps.Count,
            Steps = steps,
            TotalTokensUsed = totalTokens,
            ReachedMaxIterations = reachedMax
        });
    }
}
