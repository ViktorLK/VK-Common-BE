using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Materialization.Internal;

/// <summary>
/// Onion middleware in Psyche executing LLM calls, extracting JSON, validating contracts, and managing multi-tier self-healing retries.
/// </summary>
internal sealed class DefaultMaterializationMiddleware(
    IVKMaterializationValidator validator,
    IVKMaterializationBinder binder,
    IVKMaterializationRetryPolicy retryPolicy,
    IVKJsonSerializer jsonSerializer,
    IOptions<VKMaterializationOptions> materializationOptions) : IVKPsycheMiddleware // [AP.01]
{
    private readonly IVKMaterializationValidator _validator = VKGuard.NotNull(validator);
    private readonly IVKMaterializationBinder _binder = VKGuard.NotNull(binder);
    private readonly IVKMaterializationRetryPolicy _retryPolicy = VKGuard.NotNull(retryPolicy);
    private readonly IVKJsonSerializer _jsonSerializer = VKGuard.NotNull(jsonSerializer);
    private readonly VKMaterializationOptions _materializationOptions = VKGuard.NotNull(materializationOptions).Value;

    public int MiddlewareOrder => VKPsychePipelineScheduler.Middleware.EidosContract;

    public async Task<VKResult> InvokeAsync(
        VKPsycheContext context,
        VKPipelineDelegate next,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        VKGuard.NotNull(next);

        var contract = context.State<VKAIEidosResponseContract>();
        if (contract is null)
        {
            return await next().ConfigureAwait(false); // [CS.03]
        }

        var negotiation = context.State<VKAIEidosNegotiationResult>();
        var initialMode = negotiation?.SelectedMode ?? VKAIEidosExpressionMode.StructuredOutput;
        var currentMode = initialMode;

        var startTime = Stopwatch.GetTimestamp();
        var repairAttempt = 0;
        var accumulatedIssues = new List<string>();
        var accumulatedErrors = new List<VKMaterializationValidationError>();
        var eidosArgs = context.Args<VKAIEidosRequestArgs>();

        var maxAttempts = Math.Clamp(_materializationOptions.MaxTotalAttempts, 1, 10);
        var totalAttempts = 0;

        while (totalAttempts++ < maxAttempts)
        {
            // 1. Execute LLM Call in Pipeline Inner Ring
            var pipelineResult = await next().ConfigureAwait(false); // [CS.03]
            if (pipelineResult.IsFailure)
            {
                return pipelineResult;
            }

            // 2. Extract raw JSON from response
            string? rawJson = ExtractRawJson(context, contract);
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                SetEnvelope(context, null, null, currentMode, initialMode, contract, ["No JSON content extracted from response."], accumulatedErrors, repairAttempt, startTime);
                return VKResult.Success();
            }

            // 3. Validate output against schema
            var validationRes = _validator.Validate(rawJson, contract.Schema);
            var targetType = (eidosArgs?.ContractSpec as VKTypeContractSpec)?.TargetType ?? typeof(Dictionary<string, object?>);

            if (validationRes.IsSuccess && validationRes.Value.IsValid)
            {
                var bindRes = _binder.Bind(rawJson, targetType, _materializationOptions.ToleranceMode);
                var boundModel = bindRes.IsSuccess ? bindRes.Value : null;

                SetEnvelope(context, boundModel, rawJson, currentMode, initialMode, contract, accumulatedIssues, accumulatedErrors, repairAttempt, startTime);
                return VKResult.Success();
            }

            var validationResultValue = validationRes.Value ?? new VKMaterializationValidationResult { IsValid = false, ErrorMessages = ["Validation failed."] };
            accumulatedIssues.AddRange(validationResultValue.ErrorMessages);
            accumulatedErrors.AddRange(validationResultValue.Errors);

            // 4. Structured Retry Policy Evaluation
            var decision = _retryPolicy.Evaluate(repairAttempt, validationResultValue, contract.Schema, currentMode, _materializationOptions);

            switch (decision.Action)
            {
                case VKMaterializationRetryAction.PromptSelfHealing:
                    repairAttempt++;
                    if (!string.IsNullOrWhiteSpace(decision.CorrectivePrompt))
                    {
                        accumulatedIssues.Add($"[AutoRepair #{repairAttempt}] {decision.Reason}");
                        if (context.ResponseBuilder.ChatResponse?.Message is { } lastAssistantMsg)
                        {
                            context.ResponseBuilder.Messages.Add(lastAssistantMsg);
                        }
                        context.ResponseBuilder.Messages.Add(VKChatMessage.FromText(VKChatRole.User, decision.CorrectivePrompt));
                    }
                    continue;

                case VKMaterializationRetryAction.AcceptPartial:
                    var partialBindRes = _binder.Bind(rawJson, targetType, VKMaterializationToleranceMode.Partial);
                    var partialBound = partialBindRes.IsSuccess ? partialBindRes.Value : null;

                    SetEnvelope(context, partialBound, rawJson, currentMode, initialMode, contract, accumulatedIssues, accumulatedErrors, repairAttempt, startTime);
                    return VKResult.Success();

                case VKMaterializationRetryAction.Abort:
                default:
                    SetEnvelope(context, null, rawJson, currentMode, initialMode, contract, accumulatedIssues, accumulatedErrors, repairAttempt, startTime);
                    return VKResult.Success();
            }
        }

        // Circuit breaker: exceeded maximum allowed attempts
        accumulatedIssues.Add($"[CircuitBreaker] Exceeded maximum allowed execution attempts ({maxAttempts}).");
        SetEnvelope(context, null, null, currentMode, initialMode, contract, accumulatedIssues, accumulatedErrors, repairAttempt, startTime);
        return VKResult.Success();
    }

    private string? ExtractRawJson(VKPsycheContext context, VKAIEidosResponseContract contract)
    {
        var content = context.ResponseBuilder.ChatResponse?.Message.Content;
        if (!string.IsNullOrWhiteSpace(content))
        {
            return _binder.ExtractJsonBlock(content);
        }

        return null;
    }

    private void SetEnvelope(
        VKPsycheContext context,
        object? model,
        string? rawContent,
        VKAIEidosExpressionMode effectiveMode,
        VKAIEidosExpressionMode initialMode,
        VKAIEidosResponseContract contract,
        IReadOnlyList<string> issues,
        List<VKMaterializationValidationError> accumulatedErrors,
        int repairAttempts,
        long startTimestamp)
    {
        var envelope = VKMaterializationEnvelopeFactory.Create(
            model,
            rawContent,
            effectiveMode,
            initialMode,
            contract,
            issues,
            accumulatedErrors,
            repairAttempts,
            _materializationOptions.ToleranceMode,
            startTimestamp);

        context.ResponseBuilder.Metadata[EidosPsycheResponseExtensions.ModelResultMetadataKey] = model ?? envelope;
        context.ResponseBuilder.Metadata["VKMaterializationEnvelope"] = envelope;

        // Route 2: If the materialized model declares natural language representation, project it to ChatResponse.Message.Content
        // so downstream pipeline stages (EchoSave at 900, Efferent at 200) consume clean dialogue without modifying Psyche core.
        if (model is IVKNaturalLanguageContent nl && context.ResponseBuilder.ChatResponse?.Message is { } assistantMessage)
        {
            var naturalLanguage = nl.ToNaturalLanguage();
            if (!string.IsNullOrEmpty(naturalLanguage))
            {
                context.ResponseBuilder.ChatResponse = context.ResponseBuilder.ChatResponse with
                {
                    Message = assistantMessage with { Content = naturalLanguage }
                };
            }
        }
    }
}
