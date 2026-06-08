using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Tokenics.Internal;

/// <summary>
/// Predefined error constants for the Afferent Tokenics slice.
/// Follows CS.01.
/// </summary>
internal static class TokenicsErrors
{
    public static readonly VKError BudgetExceeded = VKError.Failure(
        "Afferent.Tokenics.BudgetExceeded",
        "Input token count exceeds the configured maximum budget.");

    public static readonly VKError CountingFailed = VKError.Failure(
        "Afferent.Tokenics.CountingFailed",
        "Failed to count tokens for the input.");
}
