using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Internal implementation of the Presence Assembler.
/// Orchestrates temporal, memory, environmental, and external (Soma/Social) presence contributors
/// and performs priority-based conflict mediation.
/// Follows CS.03 (ConfigureAwait, Async), AP.01 (sealed class), and AP.03.
/// </summary>
internal sealed class PresenceAssembler : IVKPresenceAssembler
{
    private readonly IEnumerable<IVKPresenceContributor> _contributors;

    public PresenceAssembler(IEnumerable<IVKPresenceContributor> contributors)
    {
        _contributors = VKGuard.NotNull(contributors);
    }

    /// <inheritdoc />
    public async Task<VKResult<string>> AssembleTapestryAsync(
        VKCognitivePipelineContext pipelineContext,
        VKPresenceState coreState,
        CancellationToken cancellationToken = default) // [CS.03]
    {
        VKGuard.NotNull(pipelineContext);
        VKGuard.NotNull(coreState);

        // Resolve TenantId and UserId
        string? tenantId = null;
        if (pipelineContext.Args?.Context != null && pipelineContext.Args.Context.TryGetValue("TenantId", out var tObj))
        {
            tenantId = tObj?.ToString();
        }
        string? userId = pipelineContext.Args?.UserId;

        // Compile core metadata
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("### [PRESENCE & WORKING MEMORY CONTEXT]");
        sb.AppendLine($"- Current UTC Time: {coreState.CurrentTime:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"- Day of Week: {coreState.DayOfWeek}");
        sb.AppendLine($"- Session Context: {(coreState.IsBusinessHours ? "Within standard business hours" : "Outside standard business hours")}");
        sb.AppendLine($"- Active Environment: {coreState.Environment ?? "Production"}");
        sb.AppendLine($"- Current Execution Stage: {coreState.PipelineStage ?? "Reasoning"}");
        sb.AppendLine($"- Active Tenant: {tenantId ?? "Default"}");
        sb.AppendLine($"- Active User: {userId ?? "Anonymous"}");
        sb.AppendLine($"- Physical Location: {coreState.WorldState.Location}");
        sb.AppendLine($"- User Activity: {coreState.WorldState.UserActivity}");
        if (coreState.WorldState.AmbientTags.Count > 0)
        {
            sb.AppendLine($"- Environment Tags: {string.Join(", ", coreState.WorldState.AmbientTags)}");
        }
        sb.AppendLine($"- Session Token RAM Used: {coreState.TotalTokensUsed} tokens (Prompt: {coreState.TotalPromptTokensUsed}, Completion: {coreState.TotalCompletionTokensUsed})");
        sb.AppendLine($"- Remaining Token Budget: {coreState.RemainingTokenBudget} tokens before active truncation");
        sb.AppendLine($"- Active Working Memory Depth: {coreState.ActiveMessageCount} messages in sliding window");

        // Build contribution context for external contributors
        var contribContext = new VKPresenceContributionContext
        {
            SessionId = pipelineContext.SessionId,
            Input = pipelineContext.Input,
            CoreState = coreState,
            Args = pipelineContext.Args
        };

        // Collect contributions and perform Conflict Mediation (最高法院调停)
        var orderedContributors = _contributors.OrderBy(c => c.Priority).ToList();
        var resolvedClaims = new Dictionary<string, VKContributionValue>(StringComparer.OrdinalIgnoreCase);
        var promptSegments = new List<string>();

        foreach (var contributor in orderedContributors)
        {
            var contribResult = await contributor.ContributeAsync(contribContext, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (contribResult.IsFailure)
            {
                return VKResult.Failure<string>(contribResult.Errors);
            }

            var contribution = contribResult.Value;
            if (contribution is not null)
            {
                if (!string.IsNullOrWhiteSpace(contribution.PromptSegment))
                {
                    promptSegments.Add(contribution.PromptSegment.Trim());
                }

                // Merge claims based on priority score (Conflict Resolution)
                foreach (var claim in contribution.Claims)
                {
                    if (resolvedClaims.TryGetValue(claim.Key, out var existingValue))
                    {
                        // Higher priority claims override lower priority ones
                        if (claim.Value.Priority > existingValue.Priority)
                        {
                            resolvedClaims[claim.Key] = claim.Value;
                        }
                    }
                    else
                    {
                        resolvedClaims[claim.Key] = claim.Value;
                    }
                }
            }
        }

        // Render mediated claims to the System Prompt
        if (resolvedClaims.Count > 0)
        {
            sb.AppendLine("- Mediated Context Attributes:");
            foreach (var claim in resolvedClaims.OrderBy(c => c.Key))
            {
                sb.AppendLine($"  - {claim.Key}: {claim.Value.Value} (Priority: {claim.Value.Priority})");
            }
        }

        // Append contributor prompt segments
        foreach (var segment in promptSegments)
        {
            sb.AppendLine(segment);
        }

        sb.AppendLine();

        return VKResult.Success(sb.ToString());
    }
}
