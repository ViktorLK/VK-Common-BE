using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Cortex.Common.Diagnostics.Internal;
using VK.Blocks.AI.Eidos;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Resilience;
using VK.Blocks.Workflow;

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
    private readonly IOptionsSnapshot<VKAICortexOptions> _options;
    private readonly ILogger<DefaultChatTurnOrchestrator> _logger;
    private readonly IVKRetryExecutor? _retryExecutor;
    private readonly IVKTimeoutExecutor? _timeoutExecutor;
    private readonly IVKCircuitBreaker? _circuitBreaker;
    private readonly IVKContractBinder? _contractBinder;

    public DefaultChatTurnOrchestrator(
        IVKPsychePipeline psychePipeline,
        IVKCortexCorrelationAccessor correlationAccessor,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IVKJsonSerializer jsonSerializer,
        IOptionsSnapshot<VKAICortexOptions> options,
        ILogger<DefaultChatTurnOrchestrator> logger,
        IVKRetryExecutor? retryExecutor = null,
        IVKTimeoutExecutor? timeoutExecutor = null,
        IVKCircuitBreaker? circuitBreaker = null,
        IVKContractBinder? contractBinder = null)
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
        _contractBinder = contractBinder;
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

        _logger.TurnOrchestrationStarted(request.SessionId.ToString(), traceId);

        var stopwatch = Stopwatch.StartNew();
        var resiliencePolicy = request.ResiliencePolicy ?? VKCortexResilienceProfiles.ChatCompletionProfile;

        var executionResult = await ExecuteWithResilienceAsync(request, correlationContext, resiliencePolicy, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        if (executionResult.IsFailure)
        {
            _logger.TurnOrchestrationFailed(request.SessionId.ToString(), traceId, executionResult.FirstError.Description);
            return VKResult.Failure<VKChatTurnResult>(executionResult.FirstError);
        }

        var psycheResponse = executionResult.Value;
        var content = psycheResponse.ChatResponse?.Message?.Content ?? string.Empty;
        var tokensUsed = psycheResponse.ChatResponse?.Usage?.TotalTokens ?? 0;

        _logger.TurnOrchestrationCompleted(request.SessionId.ToString(), traceId, stopwatch.Elapsed.TotalMilliseconds, tokensUsed);

        return VKResult.Success(new VKChatTurnResult
        {
            Content = content,
            SessionId = request.SessionId,
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

        var baseResult = await ProcessTurnAsync(request, cancellationToken).ConfigureAwait(false);
        if (baseResult.IsFailure)
        {
            return VKResult.Failure<VKChatTurnResult<TDto>>(baseResult.FirstError);
        }

        var turnResult = baseResult.Value;

        // 1. Try adaptive Eidos contract binder first if available (handles markdown fences, schema validation, repairs)
        TDto? boundDto = null;
        if (_contractBinder is not null)
        {
            var bindResult = _contractBinder.Bind<TDto>(turnResult.Content);
            if (bindResult.IsSuccess)
            {
                boundDto = bindResult.Value;
            }
        }

        // 2. Fallback to standard serializer if Eidos is not registered or binding returned null
        boundDto ??= _jsonSerializer.Deserialize<TDto>(turnResult.Content);

        if (boundDto is null)
        {
            return VKResult.Failure<VKChatTurnResult<TDto>>(VKError.Validation("Cortex.DtoBindingFailed", typeof(TDto).Name));
        }

        return VKResult.Success(new VKChatTurnResult<TDto>
        {
            Content = turnResult.Content,
            SessionId = turnResult.SessionId,
            TraceId = turnResult.TraceId,
            TokensUsed = turnResult.TokensUsed,
            ExecutionDurationMs = turnResult.ExecutionDurationMs,
            ProfilingMetrics = turnResult.ProfilingMetrics,
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

        var psycheRequest = BuildPsycheRequest(request, correlationContext, weaveOnly: true);

        return await _psychePipeline.ExecuteAsync(psycheRequest, cancellationToken).ConfigureAwait(false);
    }

    private (VKCortexCorrelationContext Context, string TraceId) InitializeCorrelation(VKChatTurnRequest request)
    {
        var traceId = !string.IsNullOrWhiteSpace(request.TraceId)
            ? request.TraceId
            : _guidGenerator.Create().ToString("N");

        var correlationContext = VKCortexCorrelationContext.Create(traceId, request.SessionId);
        return (correlationContext, traceId);
    }

    private async Task<VKResult<VKPsycheResponse>> ExecuteWithResilienceAsync(
        VKChatTurnRequest request,
        VKCortexCorrelationContext correlationContext,
        VKStepResiliencePolicy resiliencePolicy,
        CancellationToken cancellationToken)
    {
        var psycheRequest = BuildPsycheRequest(request, correlationContext, weaveOnly: false);

        return await resiliencePolicy.ExecuteWithResilienceAsync(
            ct => _psychePipeline.ExecuteAsync(psycheRequest, ct),
            _retryExecutor,
            _timeoutExecutor,
            _circuitBreaker,
            _timeProvider,
            cancellationToken).ConfigureAwait(false);
    }

    private static VKPsycheRequest BuildPsycheRequest(
        VKChatTurnRequest request,
        VKCortexCorrelationContext correlationContext,
        bool weaveOnly)
    {
        var psycheReq = new VKPsycheRequest
        {
            SessionId = request.SessionId,
            ProfileId = request.ProfileId,
            PersonaIds = request.PersonaIds,
            DirectiveIds = request.DirectiveIds,
            KnowledgeIds = request.KnowledgeIds,
            PatternIds = request.PatternIds,
            UserInput = request.UserInput,
            CorrelationId = correlationContext.TraceId,
            WeaveOnly = weaveOnly
        }.WithArgs(correlationContext);

        if (request.TargetModelId is not null || request.Temperature.HasValue || request.TopP.HasValue || request.MaxTokens.HasValue)
        {
            psycheReq = psycheReq.WithArgs(new VKChatArgs
            {
                ModelId = request.TargetModelId,
                Temperature = request.Temperature ?? 0.7f,
                TopP = request.TopP,
                MaxTokens = request.MaxTokens
            });
        }

        return psycheReq;
    }
}
