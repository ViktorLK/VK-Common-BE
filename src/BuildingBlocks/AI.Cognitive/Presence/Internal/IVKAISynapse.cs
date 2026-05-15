using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Defines a synapse that can modify AI requests before they are processed by the engine.
/// Useful for injecting global context, biological signals, or safety filters.
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
