using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    public int TaskOrder => 500;
    public bool IsParallel => false;
    public int? ParallelGroup => null;

    public Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        // // [AP.01] Defensive boundary checks via VKGuard
        VKGuard.NotNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var disabledTiers = context.Args?.DisabledTiers ?? _options.DisabledTiers;
        var tierOrderOverrides = context.Args?.TierRenderOrderOverrides ?? _options.TierRenderOrderOverrides;

        int GetFragmentDepth(VKPromptFragment fragment)
        {
            if (tierOrderOverrides is not null)
            {
                var overriddenOrder = tierOrderOverrides.IndexOf(fragment.TierType);
                if (overriddenOrder >= 0)
                {
                    return overriddenOrder;
                }
            }
            return PromptLayout.DefaultRenderOrders.TryGetValue(fragment.TierType, out var defaultOrder)
                ? defaultOrder
                : (int)fragment.TierType;
        }

        // Filter and process active fragments (excluding any disabled tiers)
        var activeFragments = context.Fragments
            .Where(f => f.Content is not null && !disabledTiers.Contains(f.TierType))
            .OrderBy(f => f.RenderOrder != 0 ? f.RenderOrder : GetFragmentDepth(f))
            .ToList();

        if (activeFragments.Count == 0 && context.Fragments.Count > 0)
        {
            WeavingDiagnostics.WeavingEmptyActive(_logger, context.SessionId);
            return Task.FromResult(VKResult.Failure(VKWeavingErrors.EmptyActive));
        }

        // 1. Separate base fragments (which lay out chronological chat history & system instructions)
        // from absolute-depth bracket injections (non-system fragments with dynamic depth targets).
        var baseFragments = activeFragments
            .Where(f => f.Depth == null || f.TierType == VKPromptTierType.Echo || f.Role == VKChatRole.System)
            .ToList();

        var injections = activeFragments
            .Where(f => f.Depth != null && f.TierType != VKPromptTierType.Echo && f.Role != VKChatRole.System)
            .ToList();

        // 2. Build Base Timeline (Order-based, oldest first)
        var finalMessages = new List<VKChatMessage>();
        var systemBuilder = new StringBuilder();

        foreach (var frag in baseFragments)
        {
            if (frag.Role == VKChatRole.System && frag.Depth == null)
            {
                // Unconditional system prompts are concatenated together to maintain prompt purity
                if (systemBuilder.Length > 0)
                {
                    systemBuilder.Append(frag.Separator ?? "\n\n");
                }
                systemBuilder.Append(frag.Content);
            }
            else
            {
                string content = frag.Content!;
                if (frag.TierType == VKPromptTierType.Echo && frag.Metadata is VKEchoTrace trace)
                {
                    content = trace.Content;
                }

                finalMessages.Add(new VKChatMessage
                {
                    Role = frag.Role,
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

        // 3. Append current UserInput as the final message in the sequence to act as the generation trigger
        if (!string.IsNullOrWhiteSpace(context.UserInput))
        {
            finalMessages.Add(new VKChatMessage
            {
                Role = VKChatRole.User,
                Content = context.UserInput
            });
        }

        // 4. Perform Absolute Position Injections (SillyTavern style: separate message inserted at global depth)
        foreach (var inject in injections)
        {
            int targetDepth = inject.Depth!.Value;
            int targetIndex = Math.Clamp(finalMessages.Count - targetDepth, 0, finalMessages.Count);

            finalMessages.Insert(targetIndex, new VKChatMessage
            {
                Role = inject.Role,
                Content = inject.Content!
            });
        }

        var tapestry = new VKPromptTapestry
        {
            Messages = finalMessages,
            SystemInstructions = systemBuilder.ToString().Trim(),
            TotalEstimatedTokens = 0
        };

        context.Tapestry = tapestry;

        WeavingDiagnostics.WeavingAssembled(_logger, context.SessionId, finalMessages.Count);

        return Task.FromResult(VKResult.Success());
    }
}
