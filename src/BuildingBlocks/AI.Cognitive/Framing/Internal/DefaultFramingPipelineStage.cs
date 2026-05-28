using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Cognitive;
using VK.Blocks.AI.Cognitive.Framing.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Framing.Internal;

internal sealed class DefaultFramingPipelineStage : IVKOrchestrationPipelineStage
{
    private readonly IVKPresenceAssembler _presenceAssembler;
    private readonly IVKTokenMeter _tokenMeter;
    private readonly IVKSystemTelemetry _systemTelemetry;
    private readonly ILogger<DefaultFramingPipelineStage> _logger;
    private readonly TimeProvider? _timeProvider;

    public int Order => 500; // Executes after early governance and RAG retrieval

    public bool IsActive => true;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public DefaultFramingPipelineStage(
        IVKPresenceAssembler presenceAssembler,
        IVKTokenMeter tokenMeter,
        IVKSystemTelemetry systemTelemetry,
        ILogger<DefaultFramingPipelineStage> logger,
        TimeProvider? timeProvider = null)
    {
        _presenceAssembler = VKGuard.NotNull(presenceAssembler);
        _tokenMeter = VKGuard.NotNull(tokenMeter);
        _systemTelemetry = VKGuard.NotNull(systemTelemetry);
        _logger = VKGuard.NotNull(logger);
        _timeProvider = timeProvider;
    }

    public async Task ExecuteAsync(VKOrchestrationPipelineContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        // Read typed governance snapshot — PresenceGovernancePipelineStage MUST have executed first
        var snapshot = context.GovernanceSnapshot;
        if (snapshot is null)
        {
            context.CriticalError = VKPipelineError.From<DefaultFramingPipelineStage>(FramingErrors.GovernanceSnapshotMissing.Description);
            return;
        }

        string tenantId = snapshot.TenantId;
        var state = snapshot.State;
        string constitution = snapshot.Constitution;
        var quota = snapshot.Quota;

        FramingDiagnostics.FramingPipelineStarted(_logger, tenantId, snapshot.UserId);

        // 1. Assemble tapestry prompt (environmental tags and mediated variables)
        var tapestryResult = await _presenceAssembler.AssembleTapestryAsync(
            context,
            state,
            ct).ConfigureAwait(false);

        if (tapestryResult.IsFailure)
        {
            context.CriticalError = VKPipelineError.From<DefaultFramingPipelineStage>(tapestryResult.FirstError.Description);
            return;
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

        // Reassemble consolidated System Instruction (Ego/Persona is cleanly handled downstream by DefaultPersonaPromptExtractor)
        string consolidatedSystemPrompt = $"{systemAnchor}{l1Constitution}\n{l2Capabilities}\n{l4WorldContext}\n{thoughtGuardrail}";

        // Write back consolidated prompt to arguments so Weaving Stage can consume it
        if (context.Args is not null)
        {
            context.Args = context.Args with
            {
                // [AP.05] During the dynamic prompt evaluation/trial phase, we deliberately restrict this to l1Constitution
                // to optimize prompt adherence and avoid token bloat. Other elements are under consideration.
                SystemInstructions = l1Constitution
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
        if (context.Args is not null && await _systemTelemetry.IsProviderStressedAsync("AzureOpenAI", ct).ConfigureAwait(false))
        {
            FramingDiagnostics.FramingFallbackTriggered(_logger, 2.0);
            context.Args = context.Args with
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
            context.Args.Context["ProviderFallbackEnabled"] = true;
        }

        FramingDiagnostics.FramingPipelineCompleted(_logger, systemTokens, context.TokenBudget.AvailableHistoryLimit);
    }
}
