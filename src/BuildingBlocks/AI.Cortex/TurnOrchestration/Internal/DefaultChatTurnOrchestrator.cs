using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Cortex.Common.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Cortex.TurnOrchestration.Internal;

/// <summary>
/// Default industrial implementation of <see cref="IVKChatTurnOrchestrator"/>.
/// Pure pipeline coordinator receiving pre-resolved <see cref="VKChatTurnRequest"/> from App layers.
/// Follows AP.01, CS.01, CS.03, CS.06, CS.07, OR.01, OR.03.
/// </summary>
internal sealed class DefaultChatTurnOrchestrator : IVKChatTurnOrchestrator
{
    private readonly IVKPsychePipeline _psychePipeline;
    private readonly IVKCortexCorrelationAccessor _correlationAccessor;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly IOptionsSnapshot<VKTurnOrchestrationOptions> _options;
    private readonly ILogger<DefaultChatTurnOrchestrator> _logger;
    private readonly IVKRetryExecutor? _retryExecutor;
    private readonly IVKTimeoutExecutor? _timeoutExecutor;
    private readonly IVKCircuitBreaker? _circuitBreaker;

    public DefaultChatTurnOrchestrator(
        IVKPsychePipeline psychePipeline,
        IVKCortexCorrelationAccessor correlationAccessor,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IVKJsonSerializer jsonSerializer,
        IOptionsSnapshot<VKTurnOrchestrationOptions> options,
        ILogger<DefaultChatTurnOrchestrator> logger,
        IVKRetryExecutor? retryExecutor = null,
        IVKTimeoutExecutor? timeoutExecutor = null,
        IVKCircuitBreaker? circuitBreaker = null)
    {
        _psychePipeline = VKGuard.NotNull(psychePipeline);
        _correlationAccessor = VKGuard.NotNull(correlationAccessor);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _options = VKGuard.NotNull(options);
        _logger = VKGuard.NotNull(logger);
        _retryExecutor = retryExecutor;
        _timeoutExecutor = timeoutExecutor;
        _circuitBreaker = circuitBreaker;
    }

    /// <inheritdoc />
    public async Task<VKResult<VKChatTurnResult>> ProcessTurnAsync(
        VKChatTurnRequest request,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var (correlationContext, traceId) = InitializeCorrelation(request);
        using var scope = _correlationAccessor.BeginScope(correlationContext);

        _logger.TurnOrchestrationStarted(request.PsycheRequest.SessionId.ToString(), traceId);

        var stopwatch = Stopwatch.StartNew();
        var resiliencePolicy = ResolveResiliencePolicy(request);

        var executionResult = await ExecuteWithResilienceAsync(request, correlationContext, resiliencePolicy, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        if (executionResult.IsFailure)
        {
            _logger.TurnOrchestrationFailed(request.PsycheRequest.SessionId.ToString(), traceId, executionResult.FirstError.Description);
            return VKResult.Failure<VKChatTurnResult>(executionResult.FirstError);
        }

        var psycheResponse = executionResult.Value;
        var content = psycheResponse.ChatResponse?.Message?.Content ?? string.Empty;
        var tokensUsed = psycheResponse.ChatResponse?.Usage?.TotalTokens ?? 0;

        _logger.TurnOrchestrationCompleted(request.PsycheRequest.SessionId.ToString(), traceId, stopwatch.Elapsed.TotalMilliseconds, tokensUsed);

        return VKResult.Success(new VKChatTurnResult
        {
            Content = content,
            SessionId = request.PsycheRequest.SessionId,
            TraceId = traceId,
            TokensUsed = tokensUsed,
            ExecutionDurationMs = stopwatch.Elapsed.TotalMilliseconds,
            ProfilingMetrics = psycheResponse.ProfilingMetrics
        });
    }

    /// <inheritdoc />
    public async Task<VKResult<VKChatTurnResult<TDto>>> ProcessTurnAsync<TDto>(
        VKChatTurnRequest request,
        CancellationToken cancellationToken = default) where TDto : class
    {
        VKGuard.NotNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var (correlationContext, traceId) = InitializeCorrelation(request);
        using var scope = _correlationAccessor.BeginScope(correlationContext);

        _logger.TurnOrchestrationStarted(request.PsycheRequest.SessionId.ToString(), traceId);

        var stopwatch = Stopwatch.StartNew();
        var resiliencePolicy = ResolveResiliencePolicy(request);

        var executionResult = await ExecuteWithResilienceAsync(request, correlationContext, resiliencePolicy, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        if (executionResult.IsFailure)
        {
            _logger.TurnOrchestrationFailed(request.PsycheRequest.SessionId.ToString(), traceId, executionResult.FirstError.Description);
            return VKResult.Failure<VKChatTurnResult<TDto>>(executionResult.FirstError);
        }

        var psycheResponse = executionResult.Value;
        var content = psycheResponse.ChatResponse?.Message?.Content ?? string.Empty;
        var tokensUsed = psycheResponse.ChatResponse?.Usage?.TotalTokens ?? 0;

        // 1. Try get bound model from Metadata (set by any structured output middleware like Eidos)
        TDto? boundDto = null;
        if (psycheResponse.Metadata is { Count: > 0 })
        {
            if (psycheResponse.Metadata.TryGetValue("vk.eidos.model", out var modelObj) && modelObj is not null)
            {
                if (modelObj is TDto directDto)
                {
                    boundDto = directDto;
                }
                else
                {
                    var modelProp = modelObj.GetType().GetProperty("Model");
                    if (modelProp?.GetValue(modelObj) is TDto envelopeModel)
                    {
                        boundDto = envelopeModel;
                    }
                }
            }

            if (boundDto is null)
            {
                foreach (var value in psycheResponse.Metadata.Values)
                {
                    if (value is TDto metaDto)
                    {
                        boundDto = metaDto;
                        break;
                    }

                    var rawDtoProp = value?.GetType().GetProperty("RawDto") ?? value?.GetType().GetProperty("Model");
                    if (rawDtoProp?.GetValue(value) is TDto rawDto)
                    {
                        boundDto = rawDto;
                        break;
                    }
                }
            }
        }

        // 3. Fallback to standard serializer if no middleware set ModelResult
        if (boundDto is null && !string.IsNullOrWhiteSpace(content))
        {
            try
            {
                boundDto = _jsonSerializer.Deserialize<TDto>(content);
            }
            catch (Exception)
            {
                // Non-JSON content safely handled
            }

            if (boundDto is null)
            {
                try
                {
                    var cleanedJson = ExtractJsonBlock(content);
                    if (!string.IsNullOrWhiteSpace(cleanedJson) && !string.Equals(cleanedJson, content, StringComparison.Ordinal))
                    {
                        boundDto = _jsonSerializer.Deserialize<TDto>(cleanedJson);
                    }
                }
                catch (Exception)
                {
                    // Safe fallback
                }
            }
        }

        if (boundDto is null)
        {
            return VKResult.Failure<VKChatTurnResult<TDto>>(VKError.Validation("Cortex.DtoBindingFailed", typeof(TDto).Name));
        }

        _logger.TurnOrchestrationCompleted(request.PsycheRequest.SessionId.ToString(), traceId, stopwatch.Elapsed.TotalMilliseconds, tokensUsed);

        return VKResult.Success(new VKChatTurnResult<TDto>
        {
            Content = content,
            SessionId = request.PsycheRequest.SessionId,
            TraceId = traceId,
            TokensUsed = tokensUsed,
            ExecutionDurationMs = stopwatch.Elapsed.TotalMilliseconds,
            ProfilingMetrics = psycheResponse.ProfilingMetrics,
            Value = boundDto
        });
    }

    /// <inheritdoc />
    public async Task<VKResult<VKPsycheResponse>> PreviewPromptAsync(
        VKChatTurnRequest request,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var (correlationContext, _) = InitializeCorrelation(request);
        using var scope = _correlationAccessor.BeginScope(correlationContext);

        var psycheRequest = request.PsycheRequest.WithArgs(correlationContext) with { WeaveOnly = true };

        return await _psychePipeline.ExecuteAsync(psycheRequest, cancellationToken).ConfigureAwait(false);
    }

    private (VKCortexCorrelationContext Context, string TraceId) InitializeCorrelation(VKChatTurnRequest request)
    {
        var traceId = !string.IsNullOrWhiteSpace(request.TraceId)
            ? request.TraceId
            : _guidGenerator.Create().ToString("N");

        var correlationContext = VKCortexCorrelationContext.Create(traceId, request.PsycheRequest.SessionId);
        return (correlationContext, traceId);
    }

    private async Task<VKResult<VKPsycheResponse>> ExecuteWithResilienceAsync(
        VKChatTurnRequest request,
        VKCortexCorrelationContext correlationContext,
        VKStepResiliencePolicy resiliencePolicy,
        CancellationToken cancellationToken)
    {
        var psycheRequest = request.PsycheRequest.WithArgs(correlationContext);

        return await resiliencePolicy.ExecuteWithResilienceAsync(
            ct => _psychePipeline.ExecuteAsync(psycheRequest, ct),
            _retryExecutor,
            _timeoutExecutor,
            _circuitBreaker,
            _timeProvider,
            cancellationToken).ConfigureAwait(false);
    }

    private VKStepResiliencePolicy ResolveResiliencePolicy(VKChatTurnRequest request)
    {
        if (request.ResiliencePolicy is not null)
        {
            return request.ResiliencePolicy;
        }

        var orchestrationOptions = _options.Value;
        var timeout = orchestrationOptions?.Timeout ?? CortexConstants.Resilience.DefaultChatTimeout;
        var retryCount = orchestrationOptions?.RetryCount ?? CortexConstants.Resilience.DefaultChatMaxRetries;
        var circuitBreakerKey = orchestrationOptions?.CircuitBreakerKey ?? CortexConstants.Resilience.DefaultLlmCircuitBreakerKey;

        return VKCortexResilienceProfiles.CreateChatProfile(timeout, retryCount, circuitBreakerKey);
    }

    private static string ExtractJsonBlock(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var trimmed = text.Trim();
        var fenceStart = trimmed.IndexOf("```", StringComparison.Ordinal);
        if (fenceStart >= 0)
        {
            var lineEnd = trimmed.IndexOf('\n', fenceStart);
            if (lineEnd >= 0)
            {
                var fenceEnd = trimmed.IndexOf("```", lineEnd, StringComparison.Ordinal);
                if (fenceEnd > lineEnd)
                {
                    return trimmed.Substring(lineEnd + 1, fenceEnd - lineEnd - 1).Trim();
                }
            }
        }

        var firstBrace = trimmed.IndexOf('{');
        var lastBrace = trimmed.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return trimmed.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return trimmed;
    }
}
