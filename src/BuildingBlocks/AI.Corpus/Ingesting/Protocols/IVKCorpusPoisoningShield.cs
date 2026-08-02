using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Contract for validating document/text content against memory/knowledge poisoning, adversarial prompt injection, and oversize payloads.
/// </summary>
public interface IVKCorpusPoisoningShield
{
    /// <summary>
    /// Validates raw content text for security threats, adversarial injection patterns, and size limits.
    /// </summary>
    /// <param name="content">The text content to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the content passed safety verification.</returns>
    Task<VKResult> ValidateContentAsync(string content, CancellationToken cancellationToken = default);
}
