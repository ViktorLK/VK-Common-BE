using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI;

public static class VKAudioDiagnosticsConstants
{
    public const int AudioInitializedEventId = VKAIDiagnosticOffsets.Audio + 1;
    public const int AudioSpeechEngineInitializedEventId = VKAIDiagnosticOffsets.Audio + 2;
    public const int AudioSpeechRequestFailedEventId = VKAIDiagnosticOffsets.Audio + 3;
    public const int AudioTranscriptionEngineInitializedEventId = VKAIDiagnosticOffsets.Audio + 4;
    public const int AudioTranscriptionRequestFailedEventId = VKAIDiagnosticOffsets.Audio + 5;

    public static class Metrics
    {
        public const string AudioDuration = "vk.ai.audio.duration";
    }

    public static class Tags
    {
        public const string AudioId = "vk.ai.audio.id";
    }
}
