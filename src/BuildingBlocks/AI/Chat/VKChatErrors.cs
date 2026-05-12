using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Standard error constants for the Chat feature.
/// </summary>
public static class VKChatErrors
{
    /// <summary>
    /// Error returned when no chat engine provider is registered.
    /// </summary>
    public static readonly VKError NotImplemented = new("AI.Chat.NotImplemented", "No chat engine provider is registered.");

    /// <summary>
    /// Error returned when the chat request is invalid.
    /// </summary>
    public static readonly VKError InvalidRequest = new("AI.Chat.InvalidRequest", "The chat request is invalid.");

    /// <summary>
    /// Error returned when an unexpected error occurs during chat execution.
    /// </summary>
    public static readonly VKError ExecutionError = new("AI.Chat.ExecutionError", "An unexpected error occurred during chat execution.");

    /// <summary>
    /// Error returned when the underlying AI provider returns an error.
    /// </summary>
    public static readonly VKError ProviderError = new("AI.Chat.ProviderError", "The AI provider returned an error.");

    /// <summary>
    /// Error returned when the provider's rate limit is reached.
    /// </summary>
    public static readonly VKError RateLimitReached = new("AI.Chat.RateLimitReached", "The AI provider rate limit has been reached.");

    /// <summary>
    /// Error returned when the requested model is invalid or not found.
    /// </summary>
    public static readonly VKError InvalidModel = new("AI.Chat.InvalidModel", "The requested model is invalid or not found.");

    /// <summary>
    /// Error returned when the provider authentication fails.
    /// </summary>
    public static readonly VKError Unauthorized = new("AI.Chat.Unauthorized", "The AI provider authentication failed.");

    /// <summary>
    /// Error returned when the model's context window (token limit) is exceeded.
    /// </summary>
    public static readonly VKError ContextWindowExceeded = new("AI.Chat.ContextWindowExceeded", "The model's context window has been exceeded.");

    /// <summary>
    /// Error returned when the chat feature is disabled in configuration.
    /// </summary>
    public static readonly VKError FeatureDisabled = new("AI.Chat.FeatureDisabled", "The chat feature is disabled.");
}
