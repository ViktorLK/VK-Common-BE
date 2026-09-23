using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

/// <summary>
/// Pipeline task that scans all active segments and substitutes template placeholders with request variables.
/// Follows AP.01 and CS.01.
/// </summary>
internal sealed class DefaultPromptReplacementTask : IVKWeavingPipelineTask
{
    private const int HeavyReplacementThreshold = 256;

    private readonly IVKPromptTemplateEngine _templateEngine;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKWeavingOptions _options;

    public DefaultPromptReplacementTask(
        IVKPromptTemplateEngine templateEngine,
        IVKTokenCounter tokenCounter,
        VKWeavingOptions options)
    {
        _templateEngine = VKGuard.NotNull(templateEngine);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _options = VKGuard.NotNull(options);
    }

    public VKPipelineSchedule Schedule => new(VKWeavingTaskOrder.Replacement);

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var rawReplacements = context.Args<VKWeavingArgs>()?.Replacements;
        if (rawReplacements is null || rawReplacements.Count == 0)
        {
            return VKResult.Success();
        }

        var maxLength = context.Args<VKWeavingArgs>()?.MaxReplacementLength ?? _options.MaxReplacementLength;
        bool hasHeavyReplacement = false;
        foreach (var (key, value) in rawReplacements)
        {
            if (value is string strVal)
            {
                if (maxLength is > 0 && strVal.Length > maxLength.Value)
                {
                    return VKResult.Failure(VKWeavingErrors.ReplacementTooLong(key, maxLength.Value, strVal.Length));
                }

                if (strVal.Length > HeavyReplacementThreshold)
                {
                    hasHeavyReplacement = true;
                }
            }
        }

        var sanitize = context.Args<VKWeavingArgs>()?.SanitizeVariables ?? _options.SanitizeVariables;
        var replacements = sanitize ? SanitizeVariables(rawReplacements) : rawReplacements;

        if (context.Segments.Count > 0)
        {
            var updatedSegments = new List<VKPromptSegment>(context.Segments.Count);
            foreach (var seg in context.Segments)
            {
                if (string.IsNullOrWhiteSpace(seg.Payload.Content))
                {
                    updatedSegments.Add(seg);
                    continue;
                }

                var rendered = await _templateEngine.RenderAsync(seg.Payload.Content, replacements, cancellationToken).ConfigureAwait(false);
                if (!rendered.IsSuccess)
                {
                    updatedSegments.Add(seg);
                    continue;
                }

                // Fast-path / Slow-path token adjustment:
                // If replacement was heavy (>256 chars) and content changed, recount tokens for accurate downstream truncation.
                // Otherwise, maintain precalculated TokenCount (zero tokenizer overhead for normal lightweight requests).
                int tokenCount = seg.Payload.TokenCount;
                if (hasHeavyReplacement && !ReferenceEquals(rendered.Value, seg.Payload.Content))
                {
                    tokenCount = _tokenCounter.CountTokens(rendered.Value);
                }

                updatedSegments.Add(seg with { Payload = seg.Payload with { Content = rendered.Value, TokenCount = tokenCount } });
            }
            context.SetSegments(updatedSegments);
        }

        return VKResult.Success();
    }

    private static Dictionary<string, object?> SanitizeVariables(IDictionary<string, object?> raw)
    {
        var sanitized = new Dictionary<string, object?>(raw.Count);
        foreach (var (key, value) in raw)
        {
            if (value is string strVal)
            {
                sanitized[key] = SanitizeValue(strVal);
            }
            else
            {
                sanitized[key] = value;
            }
        }
        return sanitized;
    }

    private static string SanitizeValue(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // 1. Strip ChatML and known control tokens
        var cleaned = input
            .Replace(Common.Internal.PsycheConstants.ChatML.ImStart, string.Empty, System.StringComparison.OrdinalIgnoreCase)
            .Replace(Common.Internal.PsycheConstants.ChatML.ImEnd, string.Empty, System.StringComparison.OrdinalIgnoreCase)
            .Replace("<|endoftext|>", string.Empty, System.StringComparison.OrdinalIgnoreCase);

        // 2. Escape XML boundaries to prevent closing tag breakout
        return cleaned
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
