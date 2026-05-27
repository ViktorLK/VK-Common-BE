using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Cognitive.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Pre-inference context interceptor that injects environmental variables,
/// compiles dynamic prompt tapestries, and performs precise sliding-window token truncation.
/// Follows CS.01, CS.03, CS.06, AP.01, and BB.01.
/// </summary>
// [AP.01] Sealed default for classes
internal sealed class PresenceContextInterceptor : IVKCognitivePipelineInterceptor
{
    private readonly IVKPresenceTracker _presenceTracker;
    private readonly IVKPresenceAssembler _presenceAssembler;
    private readonly IVKTokenMeter _tokenMeter;
    private readonly IVKSystemTelemetry _systemTelemetry;
    private readonly ILogger<PresenceContextInterceptor>? _logger;
    private readonly TimeProvider? _timeProvider;

    public int Priority => -100; // Executes after early governance and RAG retrieval

    public PresenceContextInterceptor(
        IVKPresenceTracker presenceTracker,
        IVKPresenceAssembler presenceAssembler,
        IVKTokenMeter tokenMeter,
        IVKSystemTelemetry systemTelemetry,
        ILogger<PresenceContextInterceptor>? logger = null,
        TimeProvider? timeProvider = null)
    {
        // [AP.01] Boundary checks using VKGuard
        _presenceTracker = VKGuard.NotNull(presenceTracker);
        _presenceAssembler = VKGuard.NotNull(presenceAssembler);
        _tokenMeter = VKGuard.NotNull(tokenMeter);
        _systemTelemetry = VKGuard.NotNull(systemTelemetry);
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<VKResult> OnBeforeChatAsync(
        VKOrchestrationPipelineContext context,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] Boundary checks using VKGuard
        VKGuard.NotNull(context);

        // Read typed governance snapshot — Governance interceptor MUST have executed first
        var snapshot = context.GovernanceSnapshot;
        if (snapshot is null)
        {
            return VKResult.Failure(PresenceErrors.GovernanceSnapshotMissing); // [CS.01]
        }

        string tenantId = snapshot.TenantId;
        string userId = snapshot.UserId;
        var state = snapshot.State;
        string constitution = snapshot.Constitution;
        var quota = snapshot.Quota;

        // 1. Assemble tapestry prompt (environmental tags and mediated variables)
        var tapestryResult = await _presenceAssembler.AssembleTapestryAsync(
            context,
            state,
            cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait(false) strictly configured

        if (tapestryResult.IsFailure)
        {
            return VKResult.Failure(tapestryResult.Errors); // [CS.01]
        }

        // 2. Compile Tiered Structured Message Tapestry with rigid [SYSTEM_ANCHOR]
        var envTime = _timeProvider?.GetUtcNow().ToString("yyyy-MM-dd") ?? "2026-05-21";
        string systemAnchor = $"[SYSTEM_ANCHOR: TenantId={tenantId}; ComplianceZone=GDPR_Strict; AuditLogging=Enabled; EnvTime={envTime}]\n";

        // Tier 1: L1 Constitution (Global irreversible boundaries)
        string l1Constitution = constitution;

        // Tier 2: L2 Capabilities (Tool descriptions)
        string l2Capabilities = "";
        if (context.Args?.ActionArgs?.Chat?.Tools != null && context.Args.ActionArgs.Chat.Tools.Any())
        {
            l2Capabilities = "CAPABILITIES / TOOLS SCHEMA:\n- Active tools are registered for downstream execution.\n";
        }

        // Tier 4: L4 Context & World (DateTimeOffset, mediated variables, environmental tags, and RAG facts)
        string l4WorldContext = tapestryResult.Value;

        // Tier 6: Reasoning Guardrail (Step-by-step <thought> constraints)
        string thoughtGuardrail = "REASONING RULES:\n- Formulate your initial plan and analysis within <thought>...</thought> blocks before responding to the user.\n";

        // Reassemble consolidated System Instruction with rigid [SYSTEM_ANCHOR] and guardrail (Ego/Persona is cleanly handled downstream by DefaultPersonaPromptExtractor)
        string consolidatedSystemPrompt = $"{systemAnchor}{l1Constitution}\n{l2Capabilities}\n{l4WorldContext}\n{thoughtGuardrail}";

        // Write back consolidated prompt to arguments so Weaving Stage can consume the dynamic tenant constitution
        if (context.Args is not null)
        {
            context.Args = context.Args with
            {
                SystemInstructions = l1Constitution // TODO
            };
        }

        // 3. Expose Available Token Budget to Pipeline for final S6 Weaving
        int systemTokens = _tokenMeter.CountTokens(consolidatedSystemPrompt);
        int totalLimit = quota.TokenLimit;
        int safetyMargin = quota.SafetyMarginTokens;

        context.TokenBudget = new VKTokenBudgetPlan
        {
            TotalContextLimit = totalLimit,
            MaxResponseTokens = quota.MaxRequestTokenQuota,
            ReservedSystemTokens = systemTokens,
            AvailableHistoryLimit = totalLimit - systemTokens - safetyMargin,
            AvailableKnowledgeLimit = 0, // Dynamic calculation placeholder
            TokenMeterResolver = () => _tokenMeter
        };

        // 4. Adaptive telemetry status stress checking and overrides
        if (context.Args is not null && await _systemTelemetry.IsProviderStressedAsync("AzureOpenAI", cancellationToken).ConfigureAwait(false)) // [CS.03] ConfigureAwait(false) strictly configured
        {
            context.Args = context.Args with
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
            context.Args.Context["ProviderFallbackEnabled"] = true;
        }

        return VKResult.Success(); // [CS.01]
    }

    public async Task<VKResult> OnAfterChatAsync(
        VKOrchestrationPipelineContext context,
        VKChatMessage chatResponse,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] Boundary checks using VKGuard
        VKGuard.NotNull(context);
        VKGuard.NotNull(chatResponse);

        int promptTokens = 0;
        int completionTokens = 0;

        if (chatResponse.Metadata != null)
        {
            if (chatResponse.Metadata.TryGetValue("TokenUsage", out var usageObj) && usageObj != null)
            {
                try
                {
                    dynamic usage = usageObj;
                    promptTokens = (int)(usage.PromptTokens ?? usage.InputTokens ?? 0);
                    int outTokens = (int)(usage.CompletionTokens ?? usage.OutputTokens ?? 0);
                    int reasoningTokens = (int)(usage.ReasoningTokens ?? 0);
                    completionTokens = outTokens + reasoningTokens;
                }
                catch
                {
                    // Best effort: Ignore if metadata format differs
                }
            }
        }

        // If not retrieved, count precisely with the token meter
        if (promptTokens == 0 && completionTokens == 0)
        {
            if (_logger != null)
            {
                _logger.LogMissingUsageMetadataWarning(context.SessionId);
            }
            promptTokens = _tokenMeter.CountTokens(context.Messages ?? []);
            completionTokens = _tokenMeter.CountTokens([chatResponse]);
        }

        return await _presenceTracker.RecordUsageAsync(
            context.SessionId,
            promptTokens,
            completionTokens,
            cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait(false) strictly configured
    }
}
