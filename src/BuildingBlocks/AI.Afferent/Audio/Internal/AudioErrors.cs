using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Audio.Internal;

/// <summary>
/// Predefined error constants for the Afferent Audio slice.
/// Follows CS.01.
/// </summary>
internal static class AudioErrors
{
    public static readonly VKError AudioTooLong = VKError.Validation(
        "Afferent.Audio.AudioTooLong",
        "The provided audio exceeds the maximum allowed duration.");

    public static readonly VKError TranscriptionFailed = VKError.Failure(
        "Afferent.Audio.TranscriptionFailed",
        "Failed to transcribe the audio input to text.");
}
