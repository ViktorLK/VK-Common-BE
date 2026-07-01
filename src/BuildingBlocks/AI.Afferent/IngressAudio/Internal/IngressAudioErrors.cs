using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressAudio.Internal;

internal static class IngressAudioErrors
{
    public static readonly VKError AudioTooLong = VKError.Validation(
        "Afferent.IngressAudio.AudioTooLong",
        "The provided audio exceeds the maximum allowed duration.");

    public static readonly VKError TranscriptionFailed = VKError.Failure(
        "Afferent.IngressAudio.TranscriptionFailed",
        "Failed to transcribe the audio input to text.");
}
