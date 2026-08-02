using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Retrieval.Internal;

/// <summary>
/// Heuristic gating policy utilizing short length thresholds and multilingual keyword matching.
/// Optimized for high-throughput multi-tenant scenarios to reduce LLM API overhead.
/// </summary>
internal sealed class MultilingualKeywordGatingPolicy : IVKPrefetchGatingPolicy
{
    private readonly VKMemoryOptions _options;

    public MultilingualKeywordGatingPolicy(IOptions<VKMemoryOptions> options)
    {
        _options = VKGuard.NotNull(options?.Value);
    }

    public bool ShouldTriggerIntentExtraction(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string trimmed = input.Trim();
        if (trimmed.Length < _options.GatingShortLengthThreshold)
        {
            return true;
        }

        foreach (var keyword in _options.GatingKeywords)
        {
            if (trimmed.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
