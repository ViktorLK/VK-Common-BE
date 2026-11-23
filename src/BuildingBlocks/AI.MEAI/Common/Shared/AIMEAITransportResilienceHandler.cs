using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.MEAI.Common.Shared;

/// <summary>
/// DelegatingHandler applying <c>VK.Blocks.Resilience</c> transport-level retry and per-attempt timeout
/// using the shared <c>ai.model-call</c> policy without circuit-breaker state (OWN-TRANSPORT-RESILIENCE, MEA-06).
/// </summary>
// [AP.01] [OR.03] [CS.03] [CS.06]
internal sealed class AIMEAITransportResilienceHandler : DelegatingHandler
{
    private const string Http5xxErrorPrefix = "Http.5";
    private const string HttpTransientErrorCode = "Http.Transient";

    private readonly string _clientName;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIMEAITransportResilienceHandler"/> class.
    /// </summary>
    /// <param name="clientName">The named HttpClient identifier used for operation diagnostics.</param>
    /// <param name="serviceProvider">The service provider used to resolve <see cref="IVKPolicyRegistry"/> and options.</param>
    public AIMEAITransportResilienceHandler(string clientName, IServiceProvider serviceProvider)
    {
        _clientName = VKGuard.NotNullOrWhiteSpace(clientName);
        _serviceProvider = VKGuard.NotNull(serviceProvider);
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // [AP.01]
        VKGuard.NotNull(request);

        VKAIMEAITransportResilienceOptions transportOptions =
            _serviceProvider.GetService<IOptions<VKAIMEAIOptions>>()?.Value.TransportResilience
            ?? _serviceProvider.GetService<VKAIMEAIOptions>()?.TransportResilience
            ?? new VKAIMEAITransportResilienceOptions();

        VKAIOptions? aiOptions = _serviceProvider.GetService<IOptions<VKAIOptions>>()?.Value
            ?? _serviceProvider.GetService<VKAIOptions>();
        bool allowNoOp = transportOptions.AllowNoOpResilience || (aiOptions?.AllowNoOpResilience == true);

        // [CS.07] Fail fast unless AllowNoOpResilience is explicitly opted in.
        IVKPolicyRegistry? registry = _serviceProvider.GetService<IVKPolicyRegistry>();
        if (registry is null)
        {
            if (!allowNoOp)
            {
                throw new InvalidOperationException(
                    "IVKPolicyRegistry is required for AI.MEAI outbound transport resilience. Register VK.Blocks.Resilience or set AllowNoOpResilience = true.");
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        TimeProvider timeProvider = _serviceProvider.GetService<TimeProvider>() ?? TimeProvider.System;
        string pipelineName = string.IsNullOrWhiteSpace(transportOptions.PipelineName)
            ? VKAIConstants.AIModelCallPipelineName
            : transportOptions.PipelineName;

        // [OR.03] [MEA-06] Transport resilience owns per-attempt timeout and bounded transient retry only;
        // circuit state (OWN-CONN-HEALTH) and 429/401 routing decisions belong to AI.Synapse / AIFailoverChatClient.
        IVKResiliencePipeline pipeline = registry.GetOrAddPipeline(
            pipelineName,
            builder => builder
                .AddRetry(
                    backoffType: VKRetryBackoffType.Exponential,
                    maxRetries: transportOptions.MaxRetries,
                    initialDelay: transportOptions.InitialRetryDelay,
                    backoffMultiplier: transportOptions.BackoffMultiplier,
                    useJitter: true,
                    shouldRetry: ShouldRetryTransportError)
                .AddAttemptTimeout(transportOptions.AttemptTimeout)
                .Build());

        MEAIResilienceContextScope? scope = MEAIResilienceContextScope.Current;
        VKResilienceContext context = scope?.ResilienceContext ?? VKResilienceContext.Create(
            operationKey: $"http:{_clientName}:{request.Method}:{request.RequestUri?.Host ?? "unknown"}",
            traceId: Activity.Current?.TraceId.ToString(),
            operationName: "AIMEAI.HttpClient.SendAsync").WithMaxTotalAttempts(transportOptions.MaxTotalAttempts);

        if (context.HasExceededMaxTotalAttempts)
        {
            scope?.RecordHttpFailure(statusCode: null, retryAfter: null, VKAIFailureDisposition.RequestNotAccepted);
            throw new HttpRequestException(
                $"Transport resilience attempt cap ({context.MaxTotalAttempts}) exceeded before sending request on pipeline '{pipelineName}'.");
        }

        HttpResponseMessage? lastServerErrorResponse = null;
        Exception? lastTransientException = null;

        VKResult<HttpResponseMessage> result;
        try
        {
            result = await pipeline.ExecuteAsync<HttpResponseMessage>(
                async (ctx, ct) =>
                {
                    lastServerErrorResponse?.Dispose();
                    lastServerErrorResponse = null;
                    lastTransientException = null;
                    scope?.ResetAttemptDiagnostics();

                    try
                    {
                        HttpResponseMessage response = await base.SendAsync(request, ct).ConfigureAwait(false);
                        int statusCode = (int)response.StatusCode;

                        if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            TimeSpan? retryAfter = AIMEAIRetryAfterHelper.Extract(response.Headers, timeProvider);
                            if (retryAfter.HasValue)
                            {
                                ctx.RetryAfter = retryAfter;
                            }

                            scope?.RecordHttpFailure(statusCode, retryAfter, VKAIFailureDisposition.RequestNotAccepted);
                            return VKResult.Success(response);
                        }

                        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                        {
                            scope?.RecordHttpFailure(statusCode, retryAfter: null, VKAIFailureDisposition.RequestNotAccepted);
                            return VKResult.Success(response);
                        }

                        if (response.StatusCode == HttpStatusCode.RequestTimeout)
                        {
                            scope?.RecordHttpFailure(statusCode, retryAfter: null, VKAIFailureDisposition.OutcomeUnknown);
                            return VKResult.Success(response);
                        }

                        if (statusCode is >= 400 and < 500)
                        {
                            scope?.RecordHttpFailure(statusCode, retryAfter: null, VKAIFailureDisposition.TerminalRejected);
                            return VKResult.Success(response);
                        }

                        if (statusCode >= 500)
                        {
                            TimeSpan? retryAfter = AIMEAIRetryAfterHelper.Extract(response.Headers, timeProvider);
                            if (retryAfter.HasValue)
                            {
                                ctx.RetryAfter = retryAfter;
                            }

                            VKAIFailureDisposition disposition = response.StatusCode switch
                            {
                                HttpStatusCode.ServiceUnavailable or HttpStatusCode.BadGateway => VKAIFailureDisposition.RequestNotAccepted,
                                HttpStatusCode.GatewayTimeout => VKAIFailureDisposition.OutcomeUnknown,
                                _ => VKAIFailureDisposition.None
                            };

                            scope?.RecordHttpFailure(statusCode, retryAfter, disposition);
                            lastServerErrorResponse = response;
                            return VKResult.Failure<HttpResponseMessage>(
                                new VKError($"Http.{statusCode}", $"HTTP server error {statusCode}"));
                        }

                        return VKResult.Success(response);
                    }
                    catch (HttpRequestException ex) when (!cancellationToken.IsCancellationRequested)
                    {
                        VKAIFailureDisposition disposition = ClassifyHttpRequestExceptionDisposition(ex);
                        scope?.RecordHttpFailure((int?)ex.StatusCode, retryAfter: null, disposition);
                        lastTransientException = ex;
                        return VKResult.Failure<HttpResponseMessage>(
                            new VKError(HttpTransientErrorCode, ex.Message));
                    }
                    catch (TimeoutException ex) when (!cancellationToken.IsCancellationRequested)
                    {
                        scope?.RecordHttpFailure(statusCode: null, retryAfter: null, VKAIFailureDisposition.OutcomeUnknown);
                        lastTransientException = ex;
                        return VKResult.Failure<HttpResponseMessage>(
                            new VKError(HttpTransientErrorCode, ex.Message));
                    }
                    catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && !ct.IsCancellationRequested)
                    {
                        scope?.RecordHttpFailure(statusCode: null, retryAfter: null, VKAIFailureDisposition.OutcomeUnknown);
                        lastTransientException = ex.InnerException is TimeoutException timeoutEx ? timeoutEx : ex;
                        return VKResult.Failure<HttpResponseMessage>(
                            new VKError(HttpTransientErrorCode, lastTransientException.Message));
                    }
                },
                context: context,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            lastServerErrorResponse?.Dispose();
            throw;
        }

        if (result.IsSuccess)
        {
            lastServerErrorResponse?.Dispose();
            return result.Value;
        }

        if (result.FirstError.Code == VKResilienceErrors.Timeout.Code)
        {
            lastServerErrorResponse?.Dispose();
            scope?.RecordHttpFailure(statusCode: null, retryAfter: null, VKAIFailureDisposition.OutcomeUnknown);
            throw new TimeoutException(result.FirstError.Description);
        }

        if (lastServerErrorResponse is not null)
        {
            return lastServerErrorResponse;
        }

        if (lastTransientException is not null)
        {
            ExceptionDispatchInfo.Capture(lastTransientException).Throw();
        }

        throw new HttpRequestException($"Transport resilience pipeline '{pipelineName}' failed: {result.FirstError.Description}");
    }

    private static bool ShouldRetryTransportError(VKError error) =>
        error.Code.StartsWith(Http5xxErrorPrefix, StringComparison.Ordinal) ||
        error.Code.StartsWith(HttpTransientErrorCode, StringComparison.Ordinal) ||
        error.Code == VKResilienceErrors.Timeout.Code;

    private static VKAIFailureDisposition ClassifyHttpRequestExceptionDisposition(HttpRequestException exception) =>
        exception.HttpRequestError switch
        {
            HttpRequestError.NameResolutionError
                or HttpRequestError.ConnectionError
                or HttpRequestError.SecureConnectionError
                or HttpRequestError.ProxyTunnelError => VKAIFailureDisposition.RequestNotAccepted,
            _ => VKAIFailureDisposition.OutcomeUnknown
        };
}
