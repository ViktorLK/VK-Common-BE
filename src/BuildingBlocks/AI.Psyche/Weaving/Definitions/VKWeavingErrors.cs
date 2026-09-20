using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Standard error constants for prompt weaving operations.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static error definitions and constant descriptors.")]
public static class VKWeavingErrors
{
    public static readonly VKError NoTapestry = new("AI.Weaving.NoTapestry", "Tapestry was not produced by the weaving tasks.");
    public static readonly VKError EmptyActive = new("AI.Weaving.EmptyActive", "No active prompt fragments remaining after applying disabled tiers.");

    public static VKError ReplacementTooLong(string key, int maxLength, int actualLength) =>
        VKError.Validation(
            "AI.Weaving.ReplacementTooLong",
            $"Replacement value for key '{key}' has length {actualLength}, which exceeds the maximum allowed limit of {maxLength} characters.");

    public static VKError ContextBudgetExceeded(int requiredTokens, int availableTokens) =>
        VKError.Validation(
            "AI.Weaving.ContextBudgetExceeded",
            $"Prompt construction requires at least {requiredTokens} tokens, which exceeds the available budget of {availableTokens} tokens (after reserving response output tokens).");
}
