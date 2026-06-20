using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultTapestryWeavingTask : IVKWeavingTask
{
    private readonly VKWeavingOptions _options;
    private readonly ILogger<DefaultTapestryWeavingTask> _logger;

    public DefaultTapestryWeavingTask(
        IOptions<VKWeavingOptions> options,
        ILogger<DefaultTapestryWeavingTask> logger)
    {
        _options = VKGuard.NotNull(options).Value;
        _logger = VKGuard.NotNull(logger);
    }

    public int TaskOrder => VKWeavingTaskOrder.Weaving;
    public bool IsParallel => false;
    public int? ParallelGroup => null;

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        // // [AP.01] Defensive boundary checks via VKGuard
        VKGuard.NotNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _options.DisabledTiers;

        // 1. Filter active fragments
        var activeFragments = context.Fragments
            .Where(f => !string.IsNullOrWhiteSpace(f.Segment.Content) && !disabledTiers.Contains(f.TierType))
            .ToList();

        if (activeFragments.Count == 0 && context.Fragments.Count > 0)
        {
            _logger.WeavingEmptyActive(context.Request.SessionId);
            return Task.FromResult(VKResult.Failure(VKWeavingErrors.EmptyActive));
        }

        // 2. Separate base fragments (Fixed + Relative) from absolute injections
        var baseFragments = activeFragments
            .Where(f => f.Segment.AbsoluteDepth is null)
            .OrderBy(f => f.RenderOrder)
            .ToList();

        var injections = activeFragments
            .Where(f => f.Segment.AbsoluteDepth is not null)
            .OrderBy(f => f.Segment.DepthPriority)
            .ToList();

        // 4. Build Base Timeline (Order-based, oldest first)
        var finalMessages = new List<VKChatMessage>();
        var systemBuilder = new StringBuilder();

        foreach (var frag in baseFragments)
        {
            if (frag.Segment.Role == VKChatRole.System)
            {
                // Unconditional system prompts are concatenated together to maintain prompt purity
                if (systemBuilder.Length > 0)
                {
                    systemBuilder.Append(frag.Separator ?? PsycheConstants.Separators.DefaultSegment);
                }
                systemBuilder.Append(frag.Segment.Content);
            }
            else
            {
                string content = frag.Segment.Content!;
                if (frag.TierType == VKPromptTierType.Echo && frag.Metadata is VKEchoTrace trace)
                {
                    content = trace.Content;
                }

                finalMessages.Add(new VKChatMessage
                {
                    Role = frag.Segment.Role,
                    Content = content
                });
            }
        }

        // Prepend unified system instructions block at the very top of the chat message list
        if (systemBuilder.Length > 0)
        {
            finalMessages.Insert(0, new VKChatMessage
            {
                Role = VKChatRole.System,
                Content = systemBuilder.ToString().Trim()
            });
        }

        // 5. Append current UserInput as the final message in the sequence to act as the generation trigger
        if (!string.IsNullOrWhiteSpace(context.Request.UserInput))
        {
            finalMessages.Add(new VKChatMessage
            {
                Role = VKChatRole.User,
                Content = context.Request.UserInput
            });
        }

        // 6. Perform Absolute Position Injections (separate message inserted at global depth)
        foreach (var inject in injections)
        {
            int targetDepth = inject.Segment.AbsoluteDepth!.Value;
            int targetIndex = Math.Clamp(finalMessages.Count - targetDepth, 0, finalMessages.Count);

            finalMessages.Insert(targetIndex, new VKChatMessage
            {
                Role = inject.Segment.Role,
                Content = inject.Segment.Content!
            });
        }

        context.Response.Messages.AddRange(finalMessages);
        context.Response.SystemInstructions = systemBuilder.ToString().Trim();
        context.Response.TotalEstimatedTokens = 0;

        _logger.WeavingAssembled(context.Request.SessionId, finalMessages.Count);

        return Task.FromResult(VKResult.Success());
    }
}
