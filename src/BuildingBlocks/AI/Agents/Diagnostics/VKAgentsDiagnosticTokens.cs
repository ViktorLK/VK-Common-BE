using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKAgentsDiagnosticTokens
{
    public const int AgentsInitializedEventId = VKDiagnosticOffsets.AI_Agents + 1;

    public static class Metrics
    {
        public const string AgentsDuration = "vk.ai.agents.duration";
    }

    public static class Tags
    {
        public const string AgentsId = "vk.ai.agents.id";
    }
}
