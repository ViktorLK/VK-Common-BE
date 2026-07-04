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

    /// <summary>
    /// Standard date and time formats.
    /// </summary>
    public static class DateTimeFormats
    {
        /// <summary>
        /// Standard format with milliseconds (yyyy-MM-dd HH:mm:ss.fff), commonly used in logging and databases.
        /// </summary>
        public const string StandardWithMilliseconds = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// Standard format (yyyy-MM-dd HH:mm:ss).
        /// </summary>
        public const string Standard = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Standard ISO 8601 format in UTC.
        /// </summary>
        public const string Iso8601Utc = "yyyy-MM-ddTHH:mm:ssZ";

        /// <summary>
        /// Standard ISO 8601 format in UTC with milliseconds.
        /// </summary>
        public const string Iso8601UtcWithMilliseconds = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// Compact format without separators, useful for file names or identifiers.
        /// </summary>
        public const string Compact = "yyyyMMddHHmmss";
    }
}
