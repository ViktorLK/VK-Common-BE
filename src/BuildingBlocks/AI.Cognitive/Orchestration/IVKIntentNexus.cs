using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Orchestration: Intent arbitration, agent collaboration, and tool scheduling.
/// Metaphor: Nexus - The commander of behavior and decisions.
/// Value: Plugin scheduling (Industrial) and the balance between instinct and reason (PWP).
/// </summary>
public interface IVKIntentNexus
{
    /// <summary>
    /// Routes the user input to an identified intent context.
    /// </summary>
    /// <param name="input">The raw user input.</param>
    /// <param name="args">Optional AI arguments for routing context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the identified intent context.</returns>
    ValueTask<VKResult<VKIntentContext>> RouteAsync(
        string input,
        IVKAIArgs? args = null,
        CancellationToken ct = default);
}
