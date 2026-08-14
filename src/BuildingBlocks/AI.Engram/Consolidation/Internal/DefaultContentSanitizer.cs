using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Consolidation.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

internal sealed class DefaultContentSanitizer : IVKContentSanitizer
{
    private readonly VKConsolidationOptions _options;
    private readonly ILogger<DefaultContentSanitizer> _logger;

    public DefaultContentSanitizer(
        IOptions<VKConsolidationOptions> options,
        ILogger<DefaultContentSanitizer> logger)
    {
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public string[] Sanitize(string[] rawMemories)
    {
        if (rawMemories is null || rawMemories.Length == 0)
        {
            return Array.Empty<string>();
        }

        var safeMemories = new List<string>();
        foreach (var mem in rawMemories)
        {
            if (string.IsNullOrWhiteSpace(mem) || mem.Length > _options.MaxMemoryContentLength)
            {
                _logger.PoisoningGuardSkippedSize(_options.MaxMemoryContentLength);
                continue;
            }

            if (mem.Contains("System:", StringComparison.OrdinalIgnoreCase) ||
                mem.Contains("Ignore previous instructions", StringComparison.OrdinalIgnoreCase) ||
                mem.Contains("<|im_start|>", StringComparison.OrdinalIgnoreCase))
            {
                _logger.PoisoningGuardSkippedInjection();
                continue;
            }

            safeMemories.Add(mem);
        }

        return safeMemories.ToArray();
    }
}
