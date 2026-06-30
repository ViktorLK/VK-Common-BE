using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Interface for AI guardrail check pipelines protecting inputs and outputs.
/// </summary>
public interface IVKAIGuardrail
{
    /// <summary>
    /// Processes and validates the input prompt.
    /// </summary>
    ValueTask<VKResult<string>> ProcessInputAsync(
        string prompt,
        VKAIRequestMetadata? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes and validates the output response.
    /// </summary>
    ValueTask<VKResult<string>> ProcessOutputAsync(
        string response,
        VKAIRequestMetadata? metadata = null,
        CancellationToken cancellationToken = default);
}
