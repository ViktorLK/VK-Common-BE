using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Defines the public contract interface for Afferent Text configuration options.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKAfferentTextOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether Unicode normalization is enabled.
    /// </summary>
    bool EnableUnicodeNormalization { get; }

    /// <summary>
    /// Gets a value indicating whether whitespace trimming and normalization is enabled.
    /// </summary>
    bool EnableWhitespaceTrimming { get; }

    /// <summary>
    /// Gets the maximum allowed input length in characters (before chunking/processing).
    /// </summary>
    int MaxInputLength { get; }

    /// <summary>
    /// Gets the Unicode normalization form (e.g., "FormC").
    /// </summary>
    string NormalizationForm { get; }
}
