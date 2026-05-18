using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKMemorySummarizer"/>.
/// Simulates sentence-salience and bullet-point key fact extraction for context summarization.
/// </summary>
internal sealed class BasicMemorySummarizer : IVKMemorySummarizer
{
    public Task<VKResult<string>> SummarizeAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(content))
        {
            return Task.FromResult(VKResult.Success(string.Empty));
        }

        // Sentence-salience extraction simulator
        var lines = content.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 5)
            .ToList();

        if (lines.Count == 0)
        {
            return Task.FromResult(VKResult.Success(content));
        }

        var sb = new StringBuilder();
        sb.AppendLine("### Memory Summary");

        // Take first sentence/line as grounding context
        sb.AppendLine($"- Root context: {lines[0]}");

        // Filter and take lines containing high-value semantic indicators
        var keyFacts = lines.Skip(1)
            .Where(l => l.Contains("user", StringComparison.OrdinalIgnoreCase) ||
                        l.Contains("char", StringComparison.OrdinalIgnoreCase) ||
                        l.Contains("like", StringComparison.OrdinalIgnoreCase) ||
                        l.Contains("hate", StringComparison.OrdinalIgnoreCase) ||
                        l.Contains("want", StringComparison.OrdinalIgnoreCase) ||
                        l.Contains("feel", StringComparison.OrdinalIgnoreCase) ||
                        l.StartsWith('-') ||
                        l.StartsWith('*'))
            .Take(5)
            .ToList();

        if (keyFacts.Count > 0)
        {
            foreach (var fact in keyFacts)
            {
                var cleanFact = fact.TrimStart('-', '*', ' ').Trim();
                sb.AppendLine($"- {cleanFact}");
            }
        }
        else
        {
            // Fallback: take up to 3 standard lines
            foreach (var line in lines.Skip(1).Take(3))
            {
                sb.AppendLine($"- {line}");
            }
        }

        return Task.FromResult(VKResult.Success(sb.ToString().TrimEnd()));
    }
}
