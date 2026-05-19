using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Presence: Manages real-time Context Window and metadata awareness.
/// Metaphor: Awareness - Sensory presence and working memory of the "Now".
/// Value: Session state management (Industrial) and presence/environmental perception (PWP).
/// </summary>
internal interface IVKAISynapse // [AP.03]
{
    /// <summary>
    /// Gets the priority of the synapse (lower values fire first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Processes and potentially modifies the AI arguments.
    /// </summary>
    /// <param name="args">The AI arguments to process.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the potentially modified arguments.</returns>
    ValueTask<VKResult<IVKAIArgs>> ProcessAsync(IVKAIArgs args, CancellationToken ct = default); // [CS.03]
}
