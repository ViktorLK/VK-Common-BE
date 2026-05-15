using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the core protocol for "Embodied Intelligence" orchestration.
/// Follows the standard cognitive flow: Sense -> Recall -> Think -> Act.
/// </summary>
public interface IVKCognitivePipeline
{
    /// <summary>
    /// Executes the full cognitive pipeline for a given user input.
    /// </summary>
    /// <param name="input">The raw input from the user or environment.</param>
    /// <param name="args">The pipeline arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the end-to-end cognitive output.</returns>
    /// <remarks>
    /// The pipeline execution follows these stages:
    /// 1. Sense: Process biological signals and environmental context.
    /// 2. Recall: Retrieve relevant long-term and short-term memories (RAG).
    /// 3. Think: Apply persona logic, emotional state, and intent analysis.
    /// 4. Act: Execute agent actions or generate the final response.
    /// </remarks>
    Task<VKResult<VKCognitiveResult>> ExecuteAsync(
        string input,
        VKCognitivePipelineArgs? args = null,
        CancellationToken cancellationToken = default);
}
