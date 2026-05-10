using System;
using System.Net;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

/// <summary>
/// Maps Semantic Kernel and underlying provider exceptions to VK Errors.
/// Following CS.01: Result Pattern and structured error objects.
/// </summary>
internal static class AISKErrorMapper
{
    public static VKError Map(Exception exception)
    {
        return exception switch
        {
            HttpOperationException httpEx => MapHttpError(httpEx),
            UnauthorizedAccessException => VKChatErrors.Unauthorized,
            OperationCanceledException => VKError.Failure("Core.OperationCancelled", "The operation was cancelled."),
            ArgumentException => VKChatErrors.InvalidRequest,
            _ => VKChatErrors.ExecutionError
        };
    }

    private static VKError MapHttpError(HttpOperationException ex)
    {
        if (ex.ResponseContent?.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase) == true ||
            ex.Message.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase))
        {
            return VKChatErrors.ContextWindowExceeded;
        }

        return ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => VKChatErrors.Unauthorized,
            HttpStatusCode.TooManyRequests => VKChatErrors.RateLimitReached,
            HttpStatusCode.NotFound => VKChatErrors.InvalidModel,
            HttpStatusCode.BadRequest => VKChatErrors.InvalidRequest,
            _ => VKChatErrors.ProviderError
        };
    }
}
