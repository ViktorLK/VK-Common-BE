using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Configuration options for the Afferent Text feature.
/// Follows AP.01, AP.03, and BB.07.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), GenerateValidator = true, Namespace = "VK.Blocks.AI.Afferent.Text")]
public sealed partial record VKAfferentTextOptions : IVKAfferentTextOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Afferent Text is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether Unicode normalization is enabled.
    /// Defaults to true.
    /// </summary>
    public bool EnableUnicodeNormalization { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether whitespace trimming is enabled.
    /// Defaults to true.
    /// </summary>
    public bool EnableWhitespaceTrimming { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum allowed input length in characters.
    /// Defaults to 100,000.
    /// </summary>
    public int MaxInputLength { get; init; } = 100_000;

    /// <summary>
    /// Gets or sets the Unicode normalization form.
    /// Defaults to "FormC".
    /// </summary>
    public string NormalizationForm { get; init; } = "FormC";
}
