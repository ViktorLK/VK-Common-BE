using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressText.Internal;

internal static class IngressTextErrors
{
    public static readonly VKError InputTooLong = VKError.Validation(
        "Afferent.IngressText.InputTooLong",
        "The provided text input exceeds the maximum allowed length.");

    public static readonly VKError NormalizationFailed = VKError.Failure(
        "Afferent.IngressText.NormalizationFailed",
        "Failed to normalize the text input.");
}
