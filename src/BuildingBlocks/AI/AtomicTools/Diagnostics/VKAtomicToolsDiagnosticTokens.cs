using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKAtomicToolsDiagnosticTokens
{
    public const int AtomicToolsInitializedEventId = VKDiagnosticOffsets.AI_AtomicTools + 1;

    public static class Metrics
    {
        public const string AtomicToolsDuration = "vk.ai.atomictools.duration";
    }

    public static class Tags
    {
        public const string AtomicToolsId = "vk.ai.atomictools.id";
    }
}
