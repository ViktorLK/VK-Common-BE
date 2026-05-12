using System;
using System.Net;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

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
            _ => VK.Blocks.AI.Internal.VKAIErrorMapper.Map(exception)
        };
    }

    private static VKError MapHttpError(HttpOperationException ex)
    {
        if (ex.ResponseContent?.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase) == true ||
            ex.Message.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase))
        {
            return VKAIErrors.ContextWindowExceeded;
        }

        return ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => VKAIErrors.AuthenticationFailed,
            HttpStatusCode.TooManyRequests => VKAIErrors.QuotaExceeded,
            HttpStatusCode.NotFound => VKAIErrors.InvalidModel,
            HttpStatusCode.BadRequest => VKAIErrors.InvalidRequest(),
            _ => VKAIErrors.ProviderError
        };
    }
}
