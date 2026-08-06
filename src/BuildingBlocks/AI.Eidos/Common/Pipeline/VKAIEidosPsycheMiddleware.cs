using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Psyche Pipeline Middleware connecting VK.Blocks.AI.Eidos contract governance to Psyche pipeline lifecycle.
/// Placed in Common/Pipeline as a cross-slice system integration component.
/// </summary>
public sealed class VKAIEidosPsycheMiddleware(
    IVKContractResolver resolver,
    IVKContractNegotiator negotiator,
    IVKContractProjector projector,
    IVKContractValidator validator,
    IVKContractBinder binder,
    IVKContractExtractor extractor,
    IVKContractRepairService repairService,
    IVKContractFallbackPolicy fallbackPolicy,
    IVKProviderCapabilityDetector capabilityDetector,
    IOptions<VKParsingOptions>? parsingOptions = null) : IVKPsycheMiddleware
{
    private readonly IVKContractResolver _resolver = VKGuard.NotNull(resolver);
    private readonly IVKContractNegotiator _negotiator = VKGuard.NotNull(negotiator);
    private readonly IVKContractProjector _projector = VKGuard.NotNull(projector);
    private readonly IVKContractValidator _validator = VKGuard.NotNull(validator);
    private readonly IVKContractBinder _binder = VKGuard.NotNull(binder);
    private readonly IVKContractExtractor _extractor = VKGuard.NotNull(extractor);
    private readonly IVKContractRepairService _repairService = VKGuard.NotNull(repairService);
    private readonly IVKContractFallbackPolicy _fallbackPolicy = VKGuard.NotNull(fallbackPolicy);
    private readonly IVKProviderCapabilityDetector _capabilityDetector = VKGuard.NotNull(capabilityDetector);
    private readonly VKParsingOptions _parsingOptions = parsingOptions?.Value ?? new VKParsingOptions();

    public int MiddlewareOrder => VKPsychePipelineScheduler.Middleware.EidosContract;

    public async Task<VKResult> InvokeAsync(
        VKPsycheContext context,
        VKPipelineDelegate next,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        VKGuard.NotNull(next);

        // 1. Resolve contract via strongly-typed VKAIEidosRequestArgs or context state
        var contract = await ResolveContractAsync(context, cancellationToken).ConfigureAwait(false);
        if (contract is null)
        {
            return await next().ConfigureAwait(false);
        }

        context.SetState(contract);

        // 2. Detect capabilities & negotiate initial expression mode
        var chatArgs = context.Args<VKChatArgs>() ?? new VKChatArgs();
        var capabilities = _capabilityDetector.DetectCapabilities(
            chatArgs.Provider?.ToString() ?? "Default",
            chatArgs.ModelId ?? "Default");

        var negotiationRes = _negotiator.Negotiate(contract, capabilities);
        var currentMode = negotiationRes.SelectedMode;

        ProjectForMode(context, contract, currentMode, chatArgs);

        int repairAttempt = 0;
        List<string> accumulatedIssues = [];

        while (true)
        {
            // 3. Execute LLM Call in Pipeline Inner Ring
            var pipelineResult = await next().ConfigureAwait(false);
            if (pipelineResult.IsFailure)
            {
                return pipelineResult;
            }

            // 4. Extract raw JSON from response
            string? rawJson = ExtractRawJson(context);
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                SetEnvelope(context, null, null, currentMode, contract, ["No JSON content extracted from response."]);
                return VKResult.Success();
            }

            // 5. Validate output against schema
            var validationRes = _validator.Validate(rawJson, contract.Schema);
            if (validationRes.IsSuccess && validationRes.Value.IsValid)
            {
                var targetType = context.Args<VKAIEidosRequestArgs>()?.TargetType ?? typeof(Dictionary<string, object?>);
                var boundModel = BindModel(rawJson, targetType);
                SetEnvelope(context, boundModel, rawJson, currentMode, contract, accumulatedIssues);
                return VKResult.Success();
            }

            var validationResultValue = validationRes.Value ?? new VKAIEidosValidationResult { IsValid = false, ErrorMessages = ["Validation failed."] };
            var currentErrors = validationResultValue.ErrorMessages;
            accumulatedIssues.AddRange(currentErrors);

            // 6. Repair attempt within same expression mode
            if (_parsingOptions.EnableAutoRepair && repairAttempt < _parsingOptions.MaxRepairAttempts)
            {
                repairAttempt++;
                var repairInst = _repairService.BuildRepairInstruction(validationResultValue, contract.Schema, repairAttempt);
                accumulatedIssues.Add($"[AutoRepair #{repairAttempt}] Sent corrective instruction to LLM.");

                // Append corrective prompt as a user turn for the retry
                context.Response.Messages.Add(VKChatMessage.FromText(VKChatRole.User, repairInst.CorrectivePrompt));
                continue;
            }

            // 7. Fallback attempt (switch expression mode)
            var fallbackMode = _fallbackPolicy.GetFallbackMode(currentMode);
            if (fallbackMode != currentMode)
            {
                accumulatedIssues.Add($"[Fallback] Mode downgraded from {currentMode} to {fallbackMode}.");
                currentMode = fallbackMode;
                repairAttempt = 0;

                ProjectForMode(context, contract, currentMode, chatArgs);
                continue;
            }

            // 8. All attempts & fallbacks exhausted, return envelope with issues
            SetEnvelope(context, null, rawJson, currentMode, contract, accumulatedIssues);
            return VKResult.Success();
        }
    }

    private async Task<VKAIEidosResponseContract?> ResolveContractAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        var contract = context.State<VKAIEidosResponseContract>();
        if (contract is not null) return contract;

        var args = context.Args<VKAIEidosRequestArgs>();
        if (args is null) return null;

        if (args.ExplicitContract is not null) return args.ExplicitContract;

        if (!string.IsNullOrWhiteSpace(args.Scenario))
        {
            var tenantId = context.Request.TenantId?.Value.ToString();
            var personaId = context.Request.PersonaId.Value.ToString();

            var contractRes = await _resolver.ResolveForContextAsync(args.Scenario, tenantId, personaId, cancellationToken).ConfigureAwait(false);
            if (contractRes.IsSuccess) return contractRes.Value;
        }

        return null;
    }

    private void ProjectForMode(VKPsycheContext context, VKAIEidosResponseContract contract, VKAIEidosExpressionMode mode, VKChatArgs chatArgs)
    {
        var injectNarrative = context.Args<VKAIEidosRequestArgs>()?.InjectNarrativeField ?? false;

        switch (mode)
        {
            case VKAIEidosExpressionMode.StructuredOutput:
                if (_projector.ProjectToIntermediateRepresentation(contract, mode, injectNarrative) is string schemaStr)
                {
                    context.Request.WithArgs(chatArgs with { ResponseSchema = schemaStr });
                }
                break;

            case VKAIEidosExpressionMode.ToolCall:
                if (_projector.ProjectToIntermediateRepresentation(contract, mode, injectNarrative) is IVKAtomicTool atomicTool)
                {
                    var existingTools = chatArgs.Tools ?? [];
                    var combinedTools = existingTools.Where(t => t.Manifest.Metadata.Name != atomicTool.Manifest.Metadata.Name)
                        .Concat([atomicTool]).ToList();

                    context.Request.WithArgs(chatArgs with
                    {
                        Tools = combinedTools,
                        ToolChoice = atomicTool.Manifest.Metadata.Name
                    });
                }
                break;

            case VKAIEidosExpressionMode.PromptJson:
                if (_projector.ProjectToIntermediateRepresentation(contract, mode, injectNarrative) is string promptInstruction)
                {
                    context.AddFragment(new VKPromptFragment
                    {
                        TierType = VKPromptTierType.Directive,
                        RenderOrder = 950,
                        Metadata = new EidosFragmentMetadata(),
                        Segment = new VKPromptSegment
                        {
                            Role = VKChatRole.System,
                            Content = promptInstruction
                        }
                    });
                }
                break;
        }
    }

    private sealed class EidosFragmentMetadata : IVKFragmentMetadata
    {
        public string Source => "AI.Eidos";
        public string Version => "1.0";
    }

    private string? ExtractRawJson(VKPsycheContext context)
    {
        var toolCall = context.Response.ChatResponse?.Message.ToolCalls?.FirstOrDefault();
        if (toolCall is not null)
        {
            return JsonSerializer.Serialize(toolCall.Arguments);
        }

        if (!string.IsNullOrWhiteSpace(context.Response.ChatResponse?.Message.Content))
        {
            return _extractor.ExtractJsonBlock(context.Response.ChatResponse.Message.Content);
        }

        return null;
    }

    private object? BindModel(string rawJson, Type targetType)
    {
        var method = typeof(IVKContractBinder).GetMethod(nameof(IVKContractBinder.Bind))?.MakeGenericMethod(targetType);
        if (method is null) return null;

        var resultObj = method.Invoke(_binder, [rawJson]);
        if (resultObj is VKResult vkResult && (bool)vkResult.GetType().GetProperty("IsSuccess")!.GetValue(vkResult)!)
        {
            return vkResult.GetType().GetProperty("Value")!.GetValue(vkResult);
        }

        return null;
    }

    private static void SetEnvelope(
        VKPsycheContext context,
        object? model,
        string? rawContent,
        VKAIEidosExpressionMode mode,
        VKAIEidosResponseContract contract,
        IReadOnlyList<string> issues)
    {
        var envelope = new VKAIEidosEnvelope<object>
        {
            Model = model,
            RawContent = rawContent,
            ExpressionMode = mode,
            ContractVersion = contract.Version,
            Issues = issues
        };

        context.Response.ModelResult = envelope;
    }
}
