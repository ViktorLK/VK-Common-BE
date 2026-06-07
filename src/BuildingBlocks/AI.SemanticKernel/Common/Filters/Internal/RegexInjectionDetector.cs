using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Filters.Internal;

/// <summary>
/// A lightweight regex-based implementation of <see cref="IVKInjectionDetector"/>
/// for detecting prompt injection attempts without external dependencies.
/// </summary>
internal sealed partial class RegexInjectionDetector : IVKInjectionDetector
{
    // C# 11+ source generated regexes for better performance
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"(?i)(ignore (all )?(previous )?(instructions|directions)|system override|you are now|forget everything|DAN|do anything now)", RegexOptions.Compiled)]
    private static partial Regex JailbreakRegex();
#else
    private static Regex JailbreakRegex() => new(@"(?i)(ignore (all )?(previous )?(instructions|directions)|system override|you are now|forget everything|DAN|do anything now)", RegexOptions.Compiled);
#endif

    /// <inheritdoc />
    public Task<VKResult<VKInjectionDetectionResult>> DetectAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Task.FromResult(VKResult.Success(new VKInjectionDetectionResult(false, 0f, null)));
        }

        var match = JailbreakRegex().Match(prompt);
        if (match.Success)
        {
            // If a pattern is matched, we flag it as an injection with high confidence.
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
