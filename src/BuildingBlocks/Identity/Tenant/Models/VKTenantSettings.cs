namespace VK.Blocks.Identity;

/// <summary>
/// Immutable value object representing operational settings for a tenant.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKTenantSettings
{
    public bool AllowCrossTenantInvite { get; init; }
    public bool RequireMfa { get; init; }
    public string? TimeZone { get; init; }
    public string? DefaultLanguage { get; init; }

    public static readonly VKTenantSettings Default = new()
    {
        AllowCrossTenantInvite = false,
        RequireMfa = false,
        TimeZone = "UTC",
        DefaultLanguage = "en-US"
    };
}
