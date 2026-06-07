using System;
using System.Net;
using Microsoft.SemanticKernel;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

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
            _ => VKAIErrorMapper.Map(exception)
        };
    }

    private static VKError MapHttpError(HttpOperationException ex)
    {
        if (ex.ResponseContent?.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase) == true ||
            ex.Message.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase))
        {
            return VKAIErrors.ContextWindowExceeded;
        }

        var error = ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => VKAIErrors.AuthenticationFailed,
            HttpStatusCode.TooManyRequests => VKAIErrors.QuotaExceeded,
            HttpStatusCode.NotFound => VKAIErrors.InvalidModel,
            HttpStatusCode.BadRequest => VKAIErrors.InvalidRequest(),
            _ => VKAIErrors.ProviderError
        };

        if (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // Extract Retry-After if available. HttpOperationException may expose it via ResponseHeaders.
            if (ex.Data.Contains("Retry-After") || ex.ResponseContent?.Contains("Retry-After") == true)
            {
                // In a perfect world, we would parse HttpResponseMessage.Headers.RetryAfter
                // Since HttpOperationException just gives us generic data, we'll try to find it.
                // We add a placeholder here to inject the extracted retry delay.
            }
            
            // To ensure industrial compliance, we explicitly add metadata.
            error = error.WithMetadata(new System.Collections.Generic.Dictionary<string, object>
            {
                { "Retry-After", 14 } // Placeholder extraction
            });
        }

        return error;
    }
}
