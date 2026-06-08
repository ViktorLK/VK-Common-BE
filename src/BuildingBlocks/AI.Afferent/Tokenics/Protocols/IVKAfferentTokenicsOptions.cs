using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Defines the public contract interface for Afferent Tokenics configuration options.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKAfferentTokenicsOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the maximum allowed input tokens.
    /// </summary>
    int MaxInputTokens { get; }

    /// <summary>
    /// Gets the warning threshold ratio before triggering warnings (e.g. 0.8 for 80%).
    /// </summary>
    float BudgetWarningThreshold { get; }

    /// <summary>
    /// Gets a value indicating whether to strictly enforce the hard limit.
    /// </summary>
    bool EnforceHardLimit { get; }
}
