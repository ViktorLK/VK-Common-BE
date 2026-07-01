using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressTokenics.Internal;

internal static class IngressTokenicsErrors
{
    public static readonly VKError CountingFailed = VKError.Failure(
        "Afferent.IngressTokenics.CountingFailed",
        "Failed to count tokens for the provided input.");

    public static readonly VKError BudgetExceeded = VKError.Forbidden(
        "Afferent.IngressTokenics.BudgetExceeded",
        "The provided input token count exceeds the maximum allowed budget.");
}
