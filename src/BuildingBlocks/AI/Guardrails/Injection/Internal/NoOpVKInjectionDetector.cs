using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Guardrails.Injection.Internal;

/// <summary>
/// No-op implementation of <see cref="IVKInjectionDetector"/>.
/// Always returns a "safe" result (no injection detected).
/// </summary>
internal sealed class NoOpVKInjectionDetector : IVKInjectionDetector
{
    // [SG Hook]
    public Task<VKResult<VKInjectionDetectionResult>> DetectAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        _ = prompt;
        _ = cancellationToken;

        var result = new VKInjectionDetectionResult(
            IsInjectionDetected: false,
            ConfidenceScore: 0.0f,
            DetectedPatternType: null);

        return Task.FromResult(VKResult.Success(result));
    }
}
