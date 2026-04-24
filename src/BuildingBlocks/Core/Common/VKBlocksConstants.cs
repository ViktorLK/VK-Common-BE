namespace VK.Blocks.Core;

/// <summary>
/// Global constants for the entire VK.Blocks framework.
/// </summary>
public static class VKBlocksConstants
{
    /// <summary>
    /// The standard prefix used for all diagnostic sources, metrics, and meters.
    /// </summary>
    public const string VKBlocksPrefix = "VK.Blocks";

    /// <summary>
    /// The standard prefix used for configuration sections in appsettings.json.
    /// </summary>
    public const string VKBlocksConfigPrefix = "VKBlocks";

    /// <summary>
    /// Placeholder for unknown identities or users.
    /// </summary>
    public const string UnknownIdentity = "Unknown";

    /// <summary>
    /// Placeholder for system-level identities.
    /// </summary>
    public const string SystemIdentity = "System";

    /// <summary>
    /// The default role name for Super Administrators.
    /// </summary>
    public const string SuperAdminRole = "SuperAdmin";
}
