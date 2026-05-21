using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Orchestrates sensory, temporal, and working memory presence states and builds the system prompt overlay.
/// Follows CS.03 and AP.03.
/// </summary>
public interface IVKPresenceAssembler
{
    /// <summary>
    /// Aggregates all registered metadata slices and compiles a structured markdown presence prompt overlay.
    /// </summary>
    /// <param name="pipelineContext">The active cognitive pipeline context.</param>
    /// <param name="coreState">The captured core presence state.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the compiled system instruction overlay prompt.</returns>
    Task<VKResult<string>> AssembleTapestryAsync(
        VKCognitivePipelineContext pipelineContext,
        VKPresenceState coreState,
        CancellationToken cancellationToken = default); // [CS.03]
}
