using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Text.Internal;

/// <summary>
/// Predefined error constants for the Afferent Text slice.
/// Follows CS.01.
/// </summary>
internal static class TextErrors
{
    public static readonly VKError InputTooLong = VKError.Validation(
        "Afferent.Text.InputTooLong",
        "The provided text input exceeds the maximum allowed length.");

    public static readonly VKError NormalizationFailed = VKError.Failure(
        "Afferent.Text.NormalizationFailed",
        "Failed to normalize the text input.");
}
