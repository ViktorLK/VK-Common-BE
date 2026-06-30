using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Interface for prompt injection and jailbreak defense detection.
/// </summary>
public interface IVKAIPromptInjectionDefender
{
    /// <summary>
    /// Analyzes the prompt for prompt injection attempts.
    /// </summary>
    ValueTask<VKResult<bool>> DetectInjectionAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}
