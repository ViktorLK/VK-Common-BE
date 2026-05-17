namespace VK.Blocks.AI;

/// <summary>
/// Provides a base implementation for AI safety settings.
/// </summary>
public sealed record VKAISafetySettings : IVKAISafetySettings
{
    /// <inheritdoc />
    public bool? EnableContentFilter { get; init; }
}
