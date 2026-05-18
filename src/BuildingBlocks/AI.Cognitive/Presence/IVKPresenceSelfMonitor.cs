using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Monitors AI's output and user reaction to adjust persona dynamics.
/// Follows AP.03 public contract rules and CS.03.
/// </summary>
public interface IVKPresenceSelfMonitor
{
    /// <summary>
    /// Evaluates a completed conversation turn and evolves persona traits.
    /// </summary>
    /// <param name="turnContext">The context data for the turn.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating operation success.</returns>
    Task<VKResult> EvaluateTurnAsync(
        VKPresenceTurnContext turnContext,
        CancellationToken cancellationToken = default); // [CS.03]
}

/// <summary>
/// Context parameters for a turn-based presence evaluation.
/// Follows AP.01 (Sealed Record).
/// </summary>
public sealed record VKPresenceTurnContext
{
    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Gets the persona identifier.
    /// </summary>
    public required string PersonaId { get; init; }

    /// <summary>
    /// Gets the user's raw message.
    /// </summary>
    public required string UserInput { get; init; }

    /// <summary>
    /// Gets the assistant's response content.
    /// </summary>
    public required string AiResponse { get; init; }

    /// <summary>
    /// Gets the user sentiment analysis score (-1.0 to 1.0).
    /// </summary>
    public required double UserSentiment { get; init; }
}
