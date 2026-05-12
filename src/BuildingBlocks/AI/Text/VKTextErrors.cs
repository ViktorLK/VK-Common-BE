using VK.Blocks.Core;

namespace VK.Blocks.AI.Text;

/// <summary>
/// Error constants for the text generation feature.
/// </summary>
public static class VKTextErrors
{
    private const string Prefix = "AI.Text";

    /// <summary>
    /// The text generation feature is disabled in configuration.
    /// </summary>
    public static readonly VKError FeatureDisabled = VKError.Failure($"{Prefix}.Disabled", "The text generation feature is disabled.");

    /// <summary>
    /// The prompt is missing or empty.
    /// </summary>
    public static readonly VKError EmptyPrompt = VKError.Validation($"{Prefix}.EmptyPrompt", "The prompt cannot be null or empty.");

    /// <summary>
    /// The engine failed to generate text.
    /// </summary>
    public static readonly VKError GenerationFailed = VKError.Failure($"{Prefix}.Failed", "Failed to generate text from the AI provider.");
}
