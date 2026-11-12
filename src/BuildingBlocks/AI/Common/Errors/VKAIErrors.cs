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

    /// <summary>
    /// Error returned when a prompt injection attempt is detected.
    /// </summary>
    public static readonly VKError PromptInjectionDetected = VKError.Failure("AI.Security.PromptInjectionDetected", "Prompt injection attack detected.");

    /// <summary>
    /// Error returned when sensitive PII is detected.
    /// </summary>
    public static readonly VKError PiiDetected = VKError.Failure("AI.Security.PiiDetected", "Sensitive personally identifiable information (PII) detected.");

    /// <summary>
    /// Error returned when a token budget is exceeded.
    /// </summary>
    public static readonly VKError TokenBudgetExceeded = VKError.Failure("AI.Governance.TokenBudgetExceeded", "AI token budget or context quota exceeded.");

    /// <summary>
    /// Error returned when embedding vector generation fails.
    /// </summary>
    public static readonly VKError EmbeddingFailed = VKError.Failure("AI.Embeddings.Failed", "Failed to generate embedding vector.");

    /// <summary>
    /// Error returned when the model returned an empty response.
    /// </summary>
    public static readonly VKError EmptyResponse = VKError.Failure("AI.Chat.EmptyResponse", "Model returned an empty response.");

    /// <summary>
    /// Error returned when deserialization into structured format fails.
    /// </summary>
    public static VKError StructuredDeserializationFailed(string detail) =>
        VKError.Failure("AI.Chat.StructuredDeserializationFailed", $"Structured deserialization failed: {detail}");

    /// <summary>
    /// Error returned when the token limit is invalid.
    /// </summary>
    public static readonly VKError InvalidTokenLimit = VKError.Validation("AI.Tokenics.InvalidTokenLimit", "Token limit must be greater than zero.");

    /// <summary>
    /// Error returned when speech synthesis fails.
    /// </summary>
    public static readonly VKError AudioSynthesisFailed = VKError.Failure("AI.Audio.SynthesisFailed", "Failed to synthesize speech audio.");

    /// <summary>
    /// Error returned when audio transcription fails.
    /// </summary>
    public static readonly VKError TranscriptionFailed = VKError.Failure("AI.Audio.TranscriptionFailed", "Failed to transcribe audio stream.");

    /// <summary>
    /// Error returned when image generation fails.
    /// </summary>
    public static readonly VKError ImageGenerationFailed = VKError.Failure("AI.Image.GenerationFailed", "Failed to generate image.");

    /// <summary>
    /// Error returned when the requested AI cache entry was not found.
    /// </summary>
    public static readonly VKError CacheNotFound = VKError.NotFound("AI.Cache.NotFound", "The requested AI cache entry was not found.");
}
