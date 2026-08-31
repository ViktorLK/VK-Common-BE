using System.Diagnostics;

namespace VK.Blocks.AI;

/// <summary>
/// OpenTelemetry GenAI 1.27+ semantic convention extension methods for <see cref="Activity"/> and <see cref="TagList"/>.
/// Follows OR.01 and BB.04.
/// </summary>
public static class VKGenAISemanticExtensions
{
    // --- OpenTelemetry GenAI Standard Semantic Keys ---
    public const string SystemKey = "gen_ai.system";
    public const string SessionIdKey = "gen_ai.session.id";
    public const string RequestModelKey = "gen_ai.request.model";
    public const string ResponseModelKey = "gen_ai.response.model";
    public const string InputTokensKey = "gen_ai.usage.input_tokens";
    public const string OutputTokensKey = "gen_ai.usage.output_tokens";
    public const string TotalTokensKey = "gen_ai.usage.total_tokens";
    public const string ErrorCodeKey = "error.type";

    /// <summary>
    /// Sets the GenAI system identifier (e.g., "psyche", "openai", "anthropic").
    /// </summary>
    public static Activity? SetGenAiSystem(this Activity? activity, string system)
    {
        activity?.SetTag(SystemKey, system);
        return activity;
    }

    /// <summary>
    /// Sets the GenAI session identifier.
    /// </summary>
    public static Activity? SetGenAiSessionId(this Activity? activity, string sessionId)
    {
        activity?.SetTag(SessionIdKey, sessionId);
        return activity;
    }

    /// <summary>
    /// Sets the GenAI request target model.
    /// </summary>
    public static Activity? SetGenAiRequestModel(this Activity? activity, string model)
    {
        activity?.SetTag(RequestModelKey, model);
        return activity;
    }

    /// <summary>
    /// Sets the GenAI response model actually used.
    /// </summary>
    public static Activity? SetGenAiResponseModel(this Activity? activity, string model)
    {
        activity?.SetTag(ResponseModelKey, model);
        return activity;
    }

    /// <summary>
    /// Sets the GenAI token usage breakdown (Input, Output, and Total).
    /// </summary>
    public static Activity? SetGenAiTokens(this Activity? activity, int inputTokens, int outputTokens, int? totalTokens = null)
    {
        if (activity is null)
            return null;

        activity.SetTag(InputTokensKey, inputTokens);
        activity.SetTag(OutputTokensKey, outputTokens);
        activity.SetTag(TotalTokensKey, totalTokens ?? (inputTokens + outputTokens));
        return activity;
    }

    /// <summary>
    /// Sets an error status and error code on the activity.
    /// </summary>
    public static Activity? SetGenAiError(this Activity? activity, string errorCode, string? description = null)
    {
        if (activity is null)
            return null;

        activity.SetStatus(ActivityStatusCode.Error, description);
        activity.SetTag(ErrorCodeKey, errorCode);
        return activity;
    }

    /// <summary>
    /// Sets the activity status to OK.
    /// </summary>
    public static Activity? SetGenAiOk(this Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
        return activity;
    }
}
