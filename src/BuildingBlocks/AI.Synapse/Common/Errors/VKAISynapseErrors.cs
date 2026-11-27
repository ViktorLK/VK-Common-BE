using System;
using System.Collections.Generic;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Error constants for the AI.Synapse building block.
/// </summary>
// [CS.01]
public static class VKAISynapseErrors
{
    public static readonly VKError NoAvailableProvider = new("AISynapse.NoAvailableProvider", "No healthy AI provider is currently available to service the request.");
    public static readonly VKError ProviderNotFound = new("AISynapse.ProviderNotFound", "The specified AI connection or provider was not found.");
    public static readonly VKError RateLimitExceeded = VKError.TooManyRequests("AISynapse.RateLimitExceeded", "AI Synapse rate limit or concurrent capacity exceeded for this connection.");
    public static readonly VKError QuotaExceeded = VKError.TooManyRequests("AISynapse.QuotaExceeded", "The tenant token budget or quota has been exhausted.");
    public static readonly VKError CapabilityMismatchOnDowngrade = VKError.Failure("AISynapse.CapabilityMismatchOnDowngrade", "The downgraded AI profile or candidate does not satisfy the required model capabilities.");
    public static readonly VKError AllProvidersFailed = new("AISynapse.AllProvidersFailed", "All candidate AI providers failed during execution and fallback.");
    public static readonly VKError InvalidConfiguration = new("AISynapse.InvalidConfiguration", "The AI Synapse connection configuration is invalid.");
    public static readonly VKError InvalidEndpoint = new("AISynapse.Security.InvalidEndpoint", "The requested AI endpoint is invalid or malformed.", VKErrorType.Validation);
    public static readonly VKError UnauthorizedConnection = new("AISynapse.Security.Unauthorized", "The target AI endpoint or BYOK credential is not authorized for this tenant.", VKErrorType.Unauthorized);
    public static readonly VKError FallbackFailed = new("AISynapse.FallbackFailed", "Primary and fallback AI actions failed.");
    public static readonly VKError RetryExhausted = new("AISynapse.RetryExhausted", "AI rate-limit retry attempts exhausted.");
    public static readonly VKError EngineNotRegistered = new("AISynapse.EngineNotRegistered", "No Keyed IVKChatEngine registered for the specified provider.");

    /// <summary>
    /// Creates a rate-limit exceeded error optionally enriched with <see cref="VKAIErrors.RetryAfterMsMetadataKey"/> metadata.
    /// </summary>
    public static VKError CreateRateLimitExceeded(TimeSpan? retryAfter = null) =>
        WithRetryAfter(RateLimitExceeded, retryAfter);

    /// <summary>
    /// Creates a quota exceeded error optionally enriched with <see cref="VKAIErrors.RetryAfterMsMetadataKey"/> metadata.
    /// </summary>
    public static VKError CreateQuotaExceeded(TimeSpan? retryAfter = null) =>
        WithRetryAfter(QuotaExceeded, retryAfter);

    /// <summary>
    /// Creates a fallback failed error without leaking internal exception details.
    /// </summary>
    public static VKError CreateFallbackFailed() => FallbackFailed;

    /// <summary>
    /// Creates a retry exhausted error with detailed message.
    /// </summary>
    public static VKError CreateRetryExhausted(string? message) =>
        new("AISynapse.RetryExhausted", message ?? "AI rate-limit retry attempts exhausted.");

    private static VKError WithRetryAfter(VKError baseError, TimeSpan? retryAfter)
    {
        if (retryAfter is { } ra && ra > TimeSpan.Zero)
        {
            return baseError.WithMetadata(new Dictionary<string, object>
            {
                [VKAIErrors.RetryAfterMsMetadataKey] = ra.TotalMilliseconds
            });
        }

        return baseError;
    }
}
