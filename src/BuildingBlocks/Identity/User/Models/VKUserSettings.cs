namespace VK.Blocks.Identity;

/// <summary>
/// Immutable value object representing personal settings and preferences for a user.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKUserSettings
{
    public string PreferredLanguage { get; init; } = "en-US";
    public string TimeZone { get; init; } = "UTC";
    public string Theme { get; init; } = "system";

    public static readonly VKUserSettings Default = new();
}
