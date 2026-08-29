namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public cross-cutting diagnostics constants and OpenTelemetry GenAI semantic tokens for AI.Psyche.
/// Follows BB.04 and OR.01.
/// </summary>
public static class VKPsycheDiagnosticsConstants
{
    /// <summary>
    /// OpenTelemetry GenAI and Psyche pipeline semantic tag keys.
    /// </summary>
    public static class Tags
    {
        public const string GenAiSystem = "gen_ai.system";
        public const string OperationName = "gen_ai.operation.name";
        public const string SessionId = "gen_ai.session.id";
        public const string CorrelationId = "gen_ai.correlation_id";
        public const string RequestModel = "gen_ai.request.model";
        public const string ResponseModel = "gen_ai.response.model";
        public const string PromptTokens = "gen_ai.usage.prompt_tokens";
        public const string CompletionTokens = "gen_ai.usage.completion_tokens";
        public const string TotalTokens = "gen_ai.usage.total_tokens";
        public const string StageName = "ai.psyche.stage";
        public const string IsSuccess = "ai.psyche.is_success";
        public const string ErrorCode = "error.code";
    }

    /// <summary>
    /// Common pipeline metrics and instrument names.
    /// </summary>
    public static class Metrics
    {
        public const string PipelineExecutionDuration = "vk.ai.psyche.pipeline.execution.duration";
        public const string StageExecutionDuration = "vk.ai.psyche.stage.duration";
        public const string LLMInvocationDuration = "vk.ai.psyche.llm.duration";
        public const string TokensConsumed = "vk.ai.psyche.tokens.consumed";
    }
}
