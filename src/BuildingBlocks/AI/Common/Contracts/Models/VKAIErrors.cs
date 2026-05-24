using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines common errors for AI building blocks.
/// Following CS.01: Error constants on Errors class.
/// </summary>
public static class VKAIErrors
{
    /// <summary>
    /// Error returned when an AI operation times out.
    /// </summary>
    public static readonly VKError Timeout = VKError.Failure("AI.Timeout", "The AI operation timed out.");

    /// <summary>
    /// Error returned when the AI service is unavailable.
    /// </summary>
    public static readonly VKError ServiceUnavailable = VKError.Failure("AI.ServiceUnavailable", "The AI service is temporarily unavailable.");

    /// <summary>
    /// Error returned when the AI provider rate limit or quota has been reached.
    /// </summary>
    public static readonly VKError QuotaExceeded = VKError.Failure("AI.QuotaExceeded", "The AI provider quota has been exceeded or rate limit reached.");

    /// <summary>
    /// Error returned when the AI provider authentication failed.
    /// </summary>
    public static readonly VKError AuthenticationFailed = VKError.Failure("AI.AuthenticationFailed", "The AI provider authentication failed. Check API keys.");

    /// <summary>
    /// Error returned when the content was filtered by the provider's safety systems.
    /// </summary>
    public static readonly VKError ContentFiltered = VKError.Failure("AI.ContentFiltered", "The content was filtered by the AI provider's safety systems.");

    /// <summary>
    /// Error returned when the model's context window has been exceeded.
    /// </summary>
    public static readonly VKError ContextWindowExceeded = VKError.Failure("AI.ContextWindowExceeded", "The model's context window has been exceeded.");

    /// <summary>
    /// Error returned when the requested model is invalid or not found.
    /// </summary>
    public static readonly VKError InvalidModel = VKError.Failure("AI.InvalidModel", "The requested model is invalid or not found.");

    /// <summary>
    /// Error returned when the AI request is invalid.
    /// </summary>
    public static VKError InvalidRequest(string? detail = null) =>
        VKError.Failure("AI.InvalidRequest", detail ?? "The AI request is invalid.");

    /// <summary>
    /// Error returned when an unexpected error occurs during AI execution.
    /// </summary>
    public static readonly VKError ExecutionError = VKError.Failure("AI.ExecutionError", "An unexpected error occurred during AI execution.");

    /// <summary>
    /// Error returned when an unexpected error occurs during AI execution with details.
    /// </summary>
    public static VKError EngineError(string detail) =>
        VKError.Failure("AI.EngineError", $"Engine error: {detail}");

    /// <summary>
    /// Error returned when the underlying AI provider returns an error.
    /// </summary>
    public static readonly VKError ProviderError = VKError.Failure("AI.ProviderError", "The AI provider returned an error.");

    /// <summary>
    /// Error returned when the content violates safety or governance policies.
    /// </summary>
    public static VKError SafetyViolation(string detail) =>
        VKError.Failure("AI.SafetyViolation", $"Safety violation: {detail}");
}
