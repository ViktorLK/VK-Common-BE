using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Guardrails.Injection.Internal;

/// <summary>
/// Lightweight regex-based implementation of IVKInjectionDetector for detecting prompt injection attempts.
/// </summary>
internal sealed partial class DefaultInjectionDetector : IVKInjectionDetector
{
    [GeneratedRegex(@"(?i)(ignore (all )?(previous )?(instructions|directions)|system override|you are now|forget everything|DAN|do anything now)", RegexOptions.Compiled)]
    private static partial Regex JailbreakRegex();

    public Task<VKResult<VKInjectionDetectionResult>> DetectAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Task.FromResult(VKResult.Success(new VKInjectionDetectionResult(false, 0f, null)));
        }

        var match = JailbreakRegex().Match(prompt);
        if (match.Success)
        {
            var result = new VKInjectionDetectionResult(
                IsInjectionDetected: true,
                ConfidenceScore: 0.95f,
                DetectedPatternType: $"JailbreakHeuristic:[{match.Value}]"
            );
            return Task.FromResult(VKResult.Success(result));
        }

        return Task.FromResult(VKResult.Success(new VKInjectionDetectionResult(false, 0f, null)));
    }
}

