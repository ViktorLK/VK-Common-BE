using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for an AI Prompt Injection Detector.
/// Detects attempts to bypass model instructions or "jailbreak" the model.
/// </summary>
public interface IVKInjectionDetector
{
    /// <summary>
    /// Checks the input prompt for potential injection attacks.
    /// </summary>
    /// <param name="prompt">The input prompt.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether an injection was detected.</returns>
    Task<VKResult<VKInjectionDetectionResult>> DetectAsync(string prompt, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of an injection detection operation.
/// </summary>
public record VKInjectionDetectionResult(bool IsInjectionDetected, float ConfidenceScore, string? DetectedPatternType);
