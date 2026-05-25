using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines a gatekeeper for the AI's internal reasoning process (Thinking vs. Speaking).
/// Allows intercepting and governing the logical chain before final output.
/// </summary>
public interface IVKThoughtStream
{
    /// <summary>
    /// Evaluates the reasoning content of an AI response.
    /// </summary>
    /// <param name="reasoning">The raw reasoning/thinking content.</param>
    /// <param name="context">The orchestration context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating if the reasoning is sound or needs intervention.</returns>
    Task<VKResult<VKThoughtEvaluation>> EvaluateAsync(
        string reasoning,
        VKIntentContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Intercepts a streaming reasoning delta.
    /// </summary>
    /// <param name="delta">The partial reasoning content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The processed delta (can be masked or redirected).</returns>
    Task<VKResult<string>> InterceptDeltaAsync(string delta, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the evaluation result of an AI's thinking process.
/// </summary>
public sealed record VKThoughtEvaluation
{
    /// <summary>
    /// Gets a value indicating whether the reasoning is approved.
    /// </summary>
    public bool IsApproved { get; init; } = true;

    /// <summary>
    /// Gets an optional redirection instruction if the thinking is flawed.
    /// </summary>
    public string? RedirectionInstruction { get; init; }

    /// <summary>
    /// Gets a set of suggested refinements for the final answer.
    /// </summary>
    public IEnumerable<string>? Refinements { get; init; }
}
