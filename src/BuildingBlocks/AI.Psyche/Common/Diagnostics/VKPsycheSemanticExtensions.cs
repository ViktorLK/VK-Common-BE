using System.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain-specific semantic extensions for Psyche distributed tracing and observability.
/// Follows OR.01 and BB.04.
/// </summary>
public static class VKPsycheSemanticExtensions
{
    public const string StageKey = "ai.psyche.stage";
    public const string CorrelationIdKey = "ai.psyche.correlation_id";
    public const string MatchedCountKey = "ai.psyche.knowledge.matched_count";
    public const string RetainedEchoKey = "ai.psyche.echo.retained_count";
    public const string TrimmedEchoKey = "ai.psyche.echo.trimmed_count";
    public const string MessageCountKey = "ai.psyche.weaving.message_count";

    /// <summary>
    /// Sets the Psyche pipeline stage name.
    /// </summary>
    public static Activity? SetPsycheStage(this Activity? activity, string stageName)
    {
        activity?.SetTag(StageKey, stageName);
        return activity;
    }

    /// <summary>
    /// Sets the correlation id for the request pipeline.
    /// </summary>
    public static Activity? SetPsycheCorrelationId(this Activity? activity, string correlationId)
    {
        activity?.SetTag(CorrelationIdKey, correlationId);
        return activity;
    }

    /// <summary>
    /// Sets the number of matched knowledge entries.
    /// </summary>
    public static Activity? SetPsycheKnowledgeCount(this Activity? activity, int matchedCount)
    {
        activity?.SetTag(MatchedCountKey, matchedCount);
        return activity;
    }

    /// <summary>
    /// Sets the dialogue echo retention statistics.
    /// </summary>
    public static Activity? SetPsycheEchoCount(this Activity? activity, int retained, int trimmed)
    {
        if (activity is null)
            return null;

        activity.SetTag(RetainedEchoKey, retained);
        activity.SetTag(TrimmedEchoKey, trimmed);
        return activity;
    }

    /// <summary>
    /// Sets the total woven message count in the prompt tapestry.
    /// </summary>
    public static Activity? SetPsycheMessageCount(this Activity? activity, int messageCount)
    {
        activity?.SetTag(MessageCountKey, messageCount);
        return activity;
    }
}
