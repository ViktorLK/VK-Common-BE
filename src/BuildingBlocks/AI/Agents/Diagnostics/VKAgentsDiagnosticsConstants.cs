namespace VK.Blocks.AI;

public static class VKAgentsDiagnosticsConstants
{
    public const int AgentsInitializedEventId = VKAIDiagnosticOffsets.Agents + 1;
    public const int AgentTaskStartedEventId = VKAIDiagnosticOffsets.Agents + 2;
    public const int AgentTaskCompletedEventId = VKAIDiagnosticOffsets.Agents + 3;
    public const int AgentTaskFailedEventId = VKAIDiagnosticOffsets.Agents + 4;
    public const int AgentIterationStartedEventId = VKAIDiagnosticOffsets.Agents + 5;
    public const int ToolCallStartedEventId = VKAIDiagnosticOffsets.Agents + 6;
    public const int ToolCallCompletedEventId = VKAIDiagnosticOffsets.Agents + 7;
    public const int UnexpectedExecutionErrorEventId = VKAIDiagnosticOffsets.Agents + 8;

    public static class Metrics
    {
        public const string AgentsDuration = "vk.ai.agents.duration";
    }

    public static class Tags
    {
        public const string AgentsId = "vk.ai.agents.id";
    }
}
