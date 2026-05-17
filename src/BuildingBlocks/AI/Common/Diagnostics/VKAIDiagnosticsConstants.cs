namespace VK.Blocks.AI;

/// <summary>
/// Constants for the AI building block diagnostics.
/// </summary>
public static class VKAIDiagnosticsConstants
{
    /// <summary>
    /// The name of the AI service.
    /// </summary>
    public const string ServiceName = "AI";

    /// <summary>
    /// Tag keys for AI diagnostics.
    /// </summary>
    public static class Tags
    {
        public const string BlockName = "vk.ai.block_name";
        public const string AgentName = "vk.ai.agent.name";
        public const string ModelName = "vk.ai.model.name";
        public const string ProviderName = "vk.ai.provider.name";
        public const string TenantId = "vk.ai.tenant_id";
        public const string Success = "vk.ai.success";
        public const string ToolName = "vk.ai.tool.name";
        public const string OperationName = "vk.ai.operation.name";
        public const string ErrorType = "vk.ai.error.type";
    }

    /// <summary>
    /// Trace operation names.
    /// </summary>
    public static class Tracing
    {
        public const string AgentExecution = "vk.ai.agent.execute";
        public const string ToolExecution = "vk.ai.tool.execute";
        public const string ChatRequest = "vk.ai.chat.request";
        public const string TextGeneration = "vk.ai.text.generate";
        public const string AudioOperation = "vk.ai.audio.op";
        public const string VectoricOperation = "vk.ai.vector.op";
    }

    /// <summary>
    /// Metric names.
    /// </summary>
    public static class Metrics
    {
        public const string AgentRunCount = "vk.ai.agent.runs";
        public const string AgentRunDuration = "vk.ai.agent.duration";
        public const string ToolCallCount = "vk.ai.tool.calls";

        public const string ChatRequestCount = "vk.ai.chat.requests";
        public const string ChatRequestDuration = "vk.ai.chat.duration";

        public const string TokenUsage = "vk.ai.tokens.usage";
        public const string TokenCost = "vk.ai.tokens.cost";

        public const string VectorOperationCount = "vk.ai.vector.operations";
        public const string AudioOperationCount = "vk.ai.audio.operations";
    }
}
