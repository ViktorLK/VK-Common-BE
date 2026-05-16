using VK.Blocks.AI.Tokenics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Token Rate Limiting feature.
/// </summary>
[VKFeature(typeof(TokenicsFeature), GenerateValidator = true)]
public sealed partial record VKLimitingOptions : IVKToggleableBlockOptions
{

    /// <summary>
    /// Gets or sets a value indicating whether Token Rate Limiting is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the global TPM (Tokens Per Minute) limit.
    /// </summary>
    public int? GlobalTpm { get; init; }

    /// <summary>
    /// Gets or sets the global RPM (Requests Per Minute) limit.
    /// </summary>
    public int? GlobalRpm { get; init; }
}
