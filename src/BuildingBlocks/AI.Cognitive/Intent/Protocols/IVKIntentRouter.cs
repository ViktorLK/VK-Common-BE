using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Domain contract for routing incoming text to an intent and associated extraction context.
/// Follows CS.01, CS.03, and AP.01.
/// </summary>
public interface IVKIntentRouter
{
    /// <summary>
    /// Routes the provided raw input string to an appropriate intent classification.
    /// </summary>
    /// <param name="input">The user input to classify.</param>
    /// <param name="args">Optional execution and parameter overrides.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the classified intent context.</returns>
    ValueTask<VKResult<VKIntentContext>> RouteAsync(
        string input,
        IVKAIArgs? args = null,
        CancellationToken ct = default);
}
