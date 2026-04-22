namespace VK.Blocks.Core.Constants.Internal;

/// <summary>
/// Internal constants used across the Core library for shared defaults and technical details.
/// </summary>
internal static class CoreConstants
{
    /// <summary>
    /// The default version used for diagnostics and metadata when not specified.
    /// </summary>
    public const string DefaultVersion = "1.0.0";

    /// <summary>
    /// Default values for pagination.
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 1000;
    }

    /// <summary>
    /// Default values for tenancy.
    /// </summary>
    public static class Tenancy
    {
        public const string DefaultHeaderName = "X-Tenant-Id";
    }

    /// <summary>
    /// Standard internal error prefixes.
    /// </summary>
    public static class Errors
    {
        public const string Prefix = "VK.Core.";
    }
}
