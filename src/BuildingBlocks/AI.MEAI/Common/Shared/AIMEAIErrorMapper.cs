using System;
using System.ClientModel;
using System.Net;
using System.Net.Http;
using Mscc.GenerativeAI.Types;
using OllamaSharp.Models.Exceptions;
using VK.Blocks.AI.MEAI.Governance.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Common.Shared;

/// <summary>
/// Maps MEAI, Azure, OpenAI, Google, Ollama, and HTTP exceptions to VK structured errors.
/// Adheres to CS.01: Typed Result and named error constants.
/// </summary>
internal static class AIMEAIErrorMapper
{
    public static VKError Map(Exception exception)
        => Map(exception, TimeProvider.System);

    public static VKError Map(Exception exception, TimeProvider timeProvider)
    {
        // [AP.01] [CS.01]
        VKGuard.NotNull(exception);
        VKGuard.NotNull(timeProvider);

        return exception switch
        {
            AISecurityException secEx => secEx.Error,
            ClientResultException clientEx => MapHttpStatusCode(
                clientEx.Status,
                clientEx,
                timeProvider,
                defaultFallback: VKAIErrors.ProviderError,
                checkContextLengthExceeded: true),
            GeminiApiTimeoutException => VKAIErrors.Timeout,
            GeminiApiException geminiEx => geminiEx.Response is { } response
                ? MapHttpStatusCode((int)response.StatusCode, geminiEx, timeProvider, defaultFallback: VKAIErrors.ProviderError)
                : VKAIErrors.ProviderError,
            OllamaException ollamaEx => VKAIErrors.InvalidRequest(ollamaEx.Message),
            HttpRequestException httpEx => httpEx.StatusCode is { } statusCode
                ? MapHttpStatusCode((int)statusCode, httpEx, timeProvider, defaultFallback: VKAIErrors.ServiceUnavailable)
                : VKAIErrors.ServiceUnavailable,
            OperationCanceledException or TimeoutException => VKAIErrors.Timeout,
            _ => VKAIErrorMapper.Map(exception)
        };
    }

    public static VKResult<T> MapToResult<T>(Exception exception)
    {
        VKError error = Map(exception);
        return VKResult.Failure<T>(error);
    }

    private static VKError MapHttpStatusCode(
        int statusCode,
        Exception exception,
        TimeProvider timeProvider,
        VKError defaultFallback,
        bool checkContextLengthExceeded = false)
    {
        return statusCode switch
        {
            (int)HttpStatusCode.Unauthorized or (int)HttpStatusCode.Forbidden => VKAIErrors.AuthenticationFailed,
            (int)HttpStatusCode.TooManyRequests => VKAIErrors.RateLimited(AIMEAIRetryAfterHelper.Extract(exception, timeProvider)),
            (int)HttpStatusCode.RequestTimeout => VKAIErrors.Timeout,
            (int)HttpStatusCode.NotFound => VKAIErrors.InvalidModel,
            (int)HttpStatusCode.BadRequest when checkContextLengthExceeded &&
                exception.Message.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase) => VKAIErrors.ContextWindowExceeded,
            (int)HttpStatusCode.BadRequest => VKAIErrors.InvalidRequest(exception.Message),
            >= (int)HttpStatusCode.InternalServerError => VKAIErrors.ServiceUnavailable,
            _ => defaultFallback
        };
    }
}
