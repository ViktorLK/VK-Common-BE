using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the interface for an intelligent agent.
/// </summary>
public interface IVKAgent
{
    /// <summary>
    /// Gets the agent name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the agent description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the instructions (system prompt) for the agent.
    /// </summary>
    string Instructions { get; }

    /// <summary>
    /// Gets the list of tools available to this agent.
    /// </summary>
    IReadOnlyList<IVKAtomicTool> Tools { get; }

    /// <summary>
    /// Gets the agent metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Executes the agent task and returns the final text response.
    /// </summary>
    /// <param name="input">The task input/instruction.</param>
    /// <param name="context">The execution context (history and variables).</param>
    /// <param name="args">The execution arguments (overrides).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the agent's work.</returns>
    Task<VKResult<string>> ExecuteAsync(
        string input,
        VKAgentExecutionContext? context = null,
        VKAgentsArgs? args = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the agent task and returns a structured <see cref="VKAgentRunResult"/> with step, tool, and usage metadata.
    /// </summary>
    /// <param name="input">The task input/instruction.</param>
    /// <param name="context">The execution context.</param>
    /// <param name="args">The execution arguments (overrides).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The structured run result.</returns>
    async Task<VKResult<VKAgentRunResult>> RunAsync(
        string input,
        VKAgentExecutionContext? context = null,
        VKAgentsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(input, context, args, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return VKResult.Failure<VKAgentRunResult>(result.FirstError);
        }

        return VKResult.Success(new VKAgentRunResult
        {
            Output = result.Value ?? string.Empty,
            ThreadId = context?.ThreadId
        });
    }

    /// <summary>
    /// Executes the agent task incrementally, streaming token deltas, tool invocation boundaries, and completion events.
    /// </summary>
    /// <param name="input">The task input/instruction.</param>
    /// <param name="context">The execution context.</param>
    /// <param name="args">The execution arguments (overrides).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async stream of typed <see cref="VKAgentStreamEvent"/> results.</returns>
    async IAsyncEnumerable<VKResult<VKAgentStreamEvent>> ExecuteStreamingAsync(
        string input,
        VKAgentExecutionContext? context = null,
        VKAgentsArgs? args = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var runResult = await RunAsync(input, context, args, cancellationToken).ConfigureAwait(false);
        if (!runResult.IsSuccess)
        {
            yield return VKResult.Failure<VKAgentStreamEvent>(runResult.FirstError);
            yield break;
        }

        var value = runResult.Value!;
        if (!string.IsNullOrEmpty(value.Output))
        {
            yield return VKResult.Success(new VKAgentStreamEvent
            {
                Kind = VKAgentStreamEventKind.TokenDelta,
                AgentName = Name,
                StepIndex = value.CompletedSteps > 0 ? value.CompletedSteps : 1,
                TextDelta = value.Output
            });
        }

        yield return VKResult.Success(new VKAgentStreamEvent
        {
            Kind = VKAgentStreamEventKind.Completed,
            AgentName = Name,
            StepIndex = value.CompletedSteps,
            TextDelta = value.Output,
            Usage = value.Usage
        });
    }
}
