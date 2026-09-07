using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Domain contract for arbitrating conflicting intent classifications or disambiguating user goals.
/// Follows CS.01, CS.03, and AP.01.
/// </summary>
public interface IVKIntentArbiter
{
    /// <summary>
    /// Resolves ambiguous or conflicting intent candidates into a definitive intent context.
    /// </summary>
    /// <param name="input">The user input string.</param>
    /// <param name="args">The arbitration arguments containing candidate intents.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the arbitrated intent context.</returns>
    Task<VKResult<VKIntentContext>> ArbitrateAsync(
        string input,
        VKIntentArbiterArgs args,
        CancellationToken ct = default);
}
