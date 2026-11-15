namespace VK.Blocks.AI;

public static class VKAtomicToolsDiagnosticsConstants
{
    public const int AtomicToolsInitializedEventId = VKAIDiagnosticOffsets.AtomicTools + 1;

    public static class Metrics
    {
        public const string AtomicToolsDuration = "vk.ai.atomictools.duration";
    }

    public static class Tags
    {
        public const string AtomicToolsId = "vk.ai.atomictools.id";
    }
}
