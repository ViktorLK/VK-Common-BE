using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Common.Shared;

/// <summary>
/// Provides unified mapping of provider-specific exceptions to VK.Blocks.AI errors.
/// Following CS.01 and industrial error handling patterns.
/// </summary>
internal static class AIErrorMapper
{
    /// <summary>
    /// Maps a generic exception to a standardized AI error.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>A specialized <see cref="VKError"/>.</returns>
    internal static VKError Map(Exception exception)
    {
        if (exception is ArgumentException or ArgumentNullException)
        {
            return VKAIErrors.InvalidRequest(exception.Message);
        }

        if (exception is TimeoutException or OperationCanceledException)
        {
            return VKAIErrors.Timeout;
        }

        string message = exception.Message.ToLowerInvariant();

        // Common rate limit patterns (429)
        if (message.Contains("rate limit") || message.Contains("429") || message.Contains("too many requests"))
        {
            return VKAIErrors.QuotaExceeded;
        }

        // Common auth patterns (401)
        if (message.Contains("unauthorized") || message.Contains("401") || message.Contains("api key"))
        {
            return VKAIErrors.AuthenticationFailed;
        }

        // Content filter patterns
        if (message.Contains("content filter") || message.Contains("safety") || message.Contains("sensitive"))
        {
            return VKAIErrors.ContentFiltered;
        }

        // Default to generic engine error
        return VKAIErrors.EngineError(exception.Message);
    }
}
