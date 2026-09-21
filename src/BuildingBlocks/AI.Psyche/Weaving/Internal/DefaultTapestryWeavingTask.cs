using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultTapestryWeavingTask : IVKWeavingPipelineTask
{
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKWeavingOptions _options;
    private readonly ILogger<DefaultTapestryWeavingTask> _logger;

    public DefaultTapestryWeavingTask(
        IVKTokenCounter tokenCounter,
        VKWeavingOptions options,
        ILogger<DefaultTapestryWeavingTask> logger)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
    }

    public VKPipelineSchedule Schedule => new(VKWeavingTaskOrder.Weaving);

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        // [AP.01] Defensive boundary checks via VKGuard
        VKGuard.NotNull(context);

        // 1. Resolve active segments
        var activeSegments = context.Segments
            .Where(s => !string.IsNullOrWhiteSpace(s.Content))
            .ToList();

        bool hasSegments = activeSegments.Count > 0;
        bool hasEchoes = context.Echoes.Count > 0;
        bool hasUserInput = !string.IsNullOrWhiteSpace(context.Request.UserInput);

        if (!hasSegments && !hasEchoes && !hasUserInput)
        {
            _logger.WeavingEmptyActive(context.Request.SessionId);
            return Task.FromResult(VKResult.Failure(VKWeavingErrors.EmptyActive));
        }

        var separator = context.Args<VKWeavingArgs>()?.SegmentSeparator ?? _options.SegmentSeparator;

        // 2. Build base chat messages (Echoes + UserInput) with precise token counts
        var baseMessages = new List<VKChatMessage>();

        if (hasEchoes)
        {
            foreach (var echo in context.Echoes.OrderBy(e => e.TurnIndex))
            {
                int echoTokens = echo.TokenCount > 0
                    ? echo.TokenCount
                    : _tokenCounter.CountTokens(echo.Content);

                baseMessages.Add(new VKChatMessage
                {
                    Role = echo.Role,
                    Content = echo.Content,
                    TokenCount = echoTokens
                });
            }
        }

        if (hasUserInput)
        {
            int userTokens = context.UserEchoTrace?.TokenCount is > 0
                ? context.UserEchoTrace.TokenCount
                : _tokenCounter.CountTokens(context.Request.UserInput);

            baseMessages.Add(new VKChatMessage
            {
                Role = VKChatRole.User,
                Content = context.Request.UserInput,
                TokenCount = userTokens
            });
        }

        int msgCount = baseMessages.Count;

        // =====================================================================
        // Phase 1: Partitioning (Static Relative World & Dynamic Timeline Slots)
        // =====================================================================

        // Resolve relative (static) segments ordered by LayoutOrder, then DepthPriority
        var relativeSegments = activeSegments
            .Where(s => s.TimelineDepth is null)
            .OrderBy(s => s.LayoutOrder)
            .ThenBy(s => s.DepthPriority)
            .ToList();

        // Partition timeline segments into slots [0 .. m]
        // Slot 0: after UserInput (tail)
        // Slot 1: before UserInput (between last echo and UserInput)
        // Slot 2..m-1: between echoes
        // Slot m: before oldest echo (depth == -1 or depth >= m)
        var timelineSegments = activeSegments
            .Where(s => s.TimelineDepth is not null)
            .OrderBy(s => s.DepthPriority)
            .ToList();

        var slots = new List<VKPromptSegment>[msgCount + 1];
        for (int s = 0; s <= msgCount; s++)
        {
            slots[s] = [];
        }

        foreach (var inject in timelineSegments)
        {
            int depth = inject.TimelineDepth!.Value;
            int slotIndex = (depth == -1 || depth >= msgCount) ? msgCount : (depth <= 0 ? 0 : depth);
            slots[slotIndex].Add(inject);
        }

        // =====================================================================
        // Phase 2: Tag Coalescing (Per-slot Contiguous Merging & Accurate Tokenics)
        // =====================================================================

        var staticMessages = CoalesceSegments(relativeSegments, separator, cancellationToken);

        var coalescedSlots = new List<VKChatMessage>[msgCount + 1];
        for (int s = 0; s <= msgCount; s++)
        {
            coalescedSlots[s] = CoalesceSegments(slots[s], separator, cancellationToken);
        }

        // =====================================================================
        // Phase 3: Timeline Interleaving & Final Assembly
        // =====================================================================

        var finalMessages = new List<VKChatMessage>();

        // 1. Static Relative World (Directives + Persona)
        finalMessages.AddRange(staticMessages);

        // 2. Dynamic Timeline World (Slots interleaved with Base Messages)
        if (msgCount == 0)
        {
            finalMessages.AddRange(coalescedSlots[0]);
        }
        else
        {
            // Slot m: immediately before oldest echo
            finalMessages.AddRange(coalescedSlots[msgCount]);

            for (int b = 0; b < msgCount; b++)
            {
                finalMessages.Add(baseMessages[b]);
                // Slot after base item b is slots[m - 1 - b]
                finalMessages.AddRange(coalescedSlots[msgCount - 1 - b]);
            }
        }

        if (finalMessages.Count == 0)
        {
            _logger.WeavingEmptyActive(context.Request.SessionId);
            return Task.FromResult(VKResult.Failure(VKWeavingErrors.EmptyActive));
        }

        int estimatedTokens = 0;
        foreach (var msg in finalMessages)
        {
            estimatedTokens += msg.TokenCount;
        }

        // =====================================================================
        // Phase 4: Final Context Budget Gate (Fail-Safe Verification)
        // =====================================================================
        var modelMetadata = context.State<VKAIModelMetadata>();
        var configuredBudget = context.Args<VKWeavingArgs>()?.MaxContextBudget ?? _options.MaxContextBudget;
        var totalLimit = configuredBudget ?? modelMetadata?.ContextWindowSize;

        if (totalLimit.HasValue)
        {
            var reservedResponse = context.Args<VKWeavingArgs>()?.ResponseReservedTokens ?? _options.ResponseReservedTokens;
            int availablePromptBudget = Math.Max(0, totalLimit.Value - reservedResponse);

            if (estimatedTokens > availablePromptBudget)
            {
                _logger.ContextBudgetExceeded(context.Request.SessionId, estimatedTokens, availablePromptBudget);
                return Task.FromResult(VKResult.Failure(VKWeavingErrors.ContextBudgetExceeded(estimatedTokens, availablePromptBudget)));
            }
        }

        context.ResponseBuilder.Messages.AddRange(finalMessages);
        context.ResponseBuilder.TotalEstimatedTokens = estimatedTokens;

        return Task.FromResult(VKResult.Success());
    }

    private static string? GetEffectiveTagName(VKPromptSegment segment)
    {
        if (!string.IsNullOrWhiteSpace(segment.TagName))
        {
            return segment.TagName;
        }

        return segment.Tier switch
        {
            VKPromptTierType.Directive => PsycheConstants.XmlTags.SystemDirective,
            VKPromptTierType.Persona => PsycheConstants.XmlTags.Persona,
            _ => null
        };
    }

    private List<VKChatMessage> CoalesceSegments(
        IEnumerable<VKPromptSegment> segments,
        string separator,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<VKChatMessage>();
        var list = segments as IList<VKPromptSegment> ?? segments.ToList();
        if (list.Count == 0)
        {
            return messages;
        }

        for (int i = 0; i < list.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = list[i];
            var tagName = GetEffectiveTagName(current);

            if (string.IsNullOrWhiteSpace(tagName))
            {
                int rawTokens = current.TokenCount > 0
                    ? current.TokenCount
                    : _tokenCounter.CountTokens(current.Content);

                messages.Add(new VKChatMessage
                {
                    Role = current.Role,
                    Content = current.Content,
                    TokenCount = rawTokens
                });
                continue;
            }

            var sectionItems = new List<string> { current.Content };

            while (i + 1 < list.Count)
            {
                var next = list[i + 1];
                var nextTag = GetEffectiveTagName(next);
                if (next.Role == current.Role &&
                    !string.IsNullOrWhiteSpace(nextTag) &&
                    string.Equals(nextTag, tagName, StringComparison.OrdinalIgnoreCase))
                {
                    sectionItems.Add(next.Content);
                    i++;
                }
                else
                {
                    break;
                }
            }

            string content = sectionItems.Count == 1
                ? VKPromptXmlBuilder.Wrap(tagName, current.Content)
                : VKPromptXmlBuilder.Wrap(tagName, sectionItems, separator);

            // Accurately compute token count for the rendered XML block
            int tokenCount = _tokenCounter.CountTokens(content);

            messages.Add(new VKChatMessage
            {
                Role = current.Role,
                Content = content,
                TokenCount = tokenCount
            });
        }

        return messages;
    }
}
