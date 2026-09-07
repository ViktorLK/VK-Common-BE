using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Cognitive;
using VK.Blocks.AI.Eidos;
using VK.Blocks.AI.Cognitive.Intent.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Intent.Internal;

/// <summary>
/// Industrial-grade implementation of <see cref="IVKIntentRouter"/>.
/// Features tiered degradation (Structured Output -> FreeText LLM -> Heuristic Fallback),
/// circuit breaker pattern, SLA timeout enforcement, and versioned result contracts.
/// Integrates with <see cref="IVKContractProjector"/> and <see cref="IVKMaterializationBinder"/> from AI.Eidos.
/// </summary>
internal sealed class IndustrialIntentRouter : IVKIntentRouter // [AP.01] sealed, [AP.03] internal
{
    private readonly IVKChatEngine _chatEngine;
    private readonly DefaultIntentOrchestrator _heuristicFallback;
    private readonly VKIntentOptions _options;
    private readonly ILogger<IndustrialIntentRouter> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IVKContractProjector? _contractProjector;
    private readonly IVKMaterializationBinder? _materializationBinder;
    private readonly Lazy<VKAIEidosResponseContract?> _contract;

    // Simple thread-safe Circuit Breaker state
    private int _consecutiveFailures;
    private long _lastTripTimestampTicks;

    public IndustrialIntentRouter(
        IVKChatEngine chatEngine,
        DefaultIntentOrchestrator heuristicFallback,
        IOptions<VKIntentOptions> options,
        ILogger<IndustrialIntentRouter> logger,
        TimeProvider? timeProvider = null,
        IVKContractProjector? contractProjector = null,
        IVKMaterializationBinder? materializationBinder = null,
        IVKSchemaFactory? schemaFactory = null)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _heuristicFallback = VKGuard.NotNull(heuristicFallback);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
        _timeProvider = timeProvider ?? TimeProvider.System;
        _contractProjector = contractProjector;
        _materializationBinder = materializationBinder;
        _contract = new Lazy<VKAIEidosResponseContract?>(() =>
        {
            return schemaFactory?.CreateContract<IntentTriageDto>(
                contractName: "IntentExtraction",
                description: "Intent, emotion, and situational context triage contract");
        });
    }

    public async ValueTask<VKResult<VKIntentContext>> RouteAsync(
        string input,
        IVKAIArgs? args = null,
        CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(input);

        // 1. Check Circuit Breaker
        if (IsCircuitTripped())
        {
            return await ExecuteHeuristicFallbackAsync(input, args, ct, "CircuitBreakerOpen").ConfigureAwait(false);
        }

        long stopwatchStart = Stopwatch.GetTimestamp();

        // 2. Setup SLA Timeout linked CTS
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(_options.ExtractionTimeoutMs));

        try
        {
            // Level 1: Structured JSON LLM Output (if preferred)
            if (_options.PreferStructuredOutput)
            {
                var structuredResult = await TryStructuredExtractionAsync(input, args, timeoutCts.Token).ConfigureAwait(false);
                if (structuredResult.IsSuccess)
                {
                    ResetCircuit();
                    long elapsedMs = Stopwatch.GetElapsedTime(stopwatchStart).Milliseconds;
                    IntentDiagnostics.IntentExtracted(_logger, structuredResult.Value.Intent.ToString(), structuredResult.Value.Confidence, "LLM.Structured", elapsedMs);
                    return structuredResult;
                }

                // If not supported by provider, log and allow fall-through to Level 2
                if (structuredResult.FirstError.Code == "AI.Chat.StructuredOutputNotSupported")
                {
                    IntentDiagnostics.StructuredOutputFallback(_logger);
                }
            }

            // Level 2: FreeText LLM Output Parsing
            var freeTextResult = await TryFreeTextExtractionAsync(input, args, timeoutCts.Token).ConfigureAwait(false);
            if (freeTextResult.IsSuccess)
            {
                ResetCircuit();
                long elapsedMs = Stopwatch.GetElapsedTime(stopwatchStart).Milliseconds;
                IntentDiagnostics.IntentExtracted(_logger, freeTextResult.Value.Intent.ToString(), freeTextResult.Value.Confidence, "LLM.FreeText", elapsedMs);
                return freeTextResult;
            }

            // Record failure for LLM route
            RecordFailure();
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // SLA Timeout hit
            RecordFailure();
            IntentDiagnostics.IntentExtractionTimeout(_logger, _options.ExtractionTimeoutMs);
            return await ExecuteHeuristicFallbackAsync(input, args, ct, "SLATimeout").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            RecordFailure();
            string inputPrefix = input.Length > 30 ? input[..30] : input;
            IntentDiagnostics.IntentExtractionFailed(_logger, ex, inputPrefix);
        }

        // Level 3: Heuristic Fallback
        return await ExecuteHeuristicFallbackAsync(input, args, ct, "LLMFailure").ConfigureAwait(false);
    }

    private async Task<VKResult<VKIntentContext>> TryStructuredExtractionAsync(
        string input,
        IVKAIArgs? args,
        CancellationToken ct)
    {
        var messages = IntentPromptBuilder.BuildStructuredPrompt(input, null, _options.MaxHistoryMessages);
        var chatArgs = ResolveChatArgs(args);

        // 1. Leverage AI.Eidos contract projector if available to ensure model-specific schema format
        var contract = _contract.Value;
        if (_contractProjector is not null && contract is not null)
        {
            chatArgs = _contractProjector.ApplyProjection(
                chatArgs ?? new VKChatArgs(),
                contract,
                VKAIEidosExpressionMode.StructuredOutput);
        }

        var response = await _chatEngine.SendStructuredAsync<IntentTriageDto>(messages, chatArgs, ct).ConfigureAwait(false);

        if (response.IsFailure)
        {
            return VKResult.Failure<VKIntentContext>(response.FirstError);
        }

        return VKResult.Success(MapFromOutput(response.Value.Data, input, "LLM.Structured"));
    }

    private async Task<VKResult<VKIntentContext>> TryFreeTextExtractionAsync(
        string input,
        IVKAIArgs? args,
        CancellationToken ct)
    {
        var messages = IntentPromptBuilder.BuildFreeTextPrompt(input, null, _options.MaxHistoryMessages);
        var chatArgs = ResolveChatArgs(args);
        var response = await _chatEngine.SendAsync(messages, chatArgs, ct).ConfigureAwait(false);

        if (response.IsFailure || string.IsNullOrWhiteSpace(response.Value.Message.Content))
        {
            return VKResult.Failure<VKIntentContext>(response.IsFailure
                ? response.FirstError
                : new VKError("AI.Cognitive.Intent.EmptyResponse", "LLM returned empty intent content."));
        }

        string rawContent = response.Value.Message.Content;

        // 1. Leverage AI.Eidos MaterializationBinder to extract and bind JSON with Lenient tolerance
        if (_materializationBinder is not null)
        {
            string jsonCandidate = _materializationBinder.ExtractJsonBlock(rawContent);
            if (!string.IsNullOrWhiteSpace(jsonCandidate))
            {
                var bindResult = _materializationBinder.Bind<IntentTriageDto>(
                    jsonCandidate,
                    VKMaterializationToleranceMode.Lenient);

                if (bindResult.IsSuccess)
                {
                    return VKResult.Success(MapFromOutput(bindResult.Value, input, "LLM.EidosBound"));
                }
            }
        }

        // 2. Fallback to compact pipe-separated response parser
        var parsedContext = IntentPromptBuilder.ParseFreeTextResponse(rawContent, input);
        if (parsedContext is null)
        {
            string prefix = rawContent.Length > 30 ? rawContent[..30] : rawContent;
            IntentDiagnostics.IntentParsingFailed(_logger, prefix);
            return VKResult.Failure<VKIntentContext>(new VKError("AI.Cognitive.Intent.ParseFailed", "Failed to parse free-text intent response."));
        }

        return VKResult.Success(parsedContext);
    }

    private VKIntentContext MapFromOutput(IntentTriageDto data, string input, string source)
    {
        if (!Enum.TryParse<VKIntent>(data.Intent, ignoreCase: true, out var intent))
        {
            intent = VKIntent.Chat;
        }

        return new VKIntentContext
        {
            Intent = intent,
            Confidence = Math.Clamp(data.Confidence, 0.0, 1.0),
            Emotion = data.Emotion,
            EmotionIntensity = data.EmotionIntensity.HasValue ? Math.Clamp(data.EmotionIntensity.Value, 1, 10) : null,
            Complexity = data.Complexity ?? "Simple",
            Urgency = data.Urgency ?? "Medium",
            Topic = data.Topic,
            Language = data.Language,
            RequiresKnowledge = data.RequiresKnowledge ?? false,
            SafetyCategory = data.SafetyCategory ?? "Safe",
            QueryCue = data.QueryCue,
            PersonaHint = data.PersonaHint,
            RefinedInput = data.RefinedInput ?? input,
            SchemaVersion = 1,
            Source = source
        };
    }

    private VKChatArgs? ResolveChatArgs(IVKAIArgs? args)
    {
        var chatArgs = args as VKChatArgs;
        if (!string.IsNullOrWhiteSpace(_options.ModelName))
        {
            chatArgs = (chatArgs ?? new VKChatArgs()) with { ModelId = _options.ModelName };
        }
        return chatArgs;
    }

    private async ValueTask<VKResult<VKIntentContext>> ExecuteHeuristicFallbackAsync(
        string input,
        IVKAIArgs? args,
        CancellationToken ct,
        string fallbackReason)
    {
        var result = await _heuristicFallback.RouteAsync(input, args, ct).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            return VKResult.Success(result.Value with
            {
                Source = $"Heuristic.{fallbackReason}"
            });
        }

        return VKResult.Success(new VKIntentContext
        {
            Intent = VKIntent.Chat,
            Confidence = 0.5,
            Complexity = "Simple",
            Urgency = "Medium",
            RequiresKnowledge = false,
            SafetyCategory = "Safe",
            RefinedInput = input,
            SchemaVersion = 1,
            Source = $"DefaultFallback.{fallbackReason}"
        });
    }

    private bool IsCircuitTripped()
    {
        if (_consecutiveFailures < _options.CircuitBreakerFailureThreshold)
        {
            return false;
        }

        long lastTrip = Interlocked.Read(ref _lastTripTimestampTicks);
        TimeSpan elapsed = _timeProvider.GetUtcNow() - new DateTimeOffset(lastTrip, TimeSpan.Zero);

        if (elapsed.TotalSeconds >= _options.CircuitBreakerRecoveryWindowSeconds)
        {
            // Enter half-open state
            IntentDiagnostics.CircuitBreakerHalfOpen(_logger);
            return false;
        }

        return true;
    }

    private void RecordFailure()
    {
        int failures = Interlocked.Increment(ref _consecutiveFailures);
        if (failures == _options.CircuitBreakerFailureThreshold)
        {
            Interlocked.Exchange(ref _lastTripTimestampTicks, _timeProvider.GetUtcNow().Ticks);
            IntentDiagnostics.CircuitBreakerTripped(_logger, failures);
        }
    }

    private void ResetCircuit()
    {
        Interlocked.Exchange(ref _consecutiveFailures, 0);
    }
}
