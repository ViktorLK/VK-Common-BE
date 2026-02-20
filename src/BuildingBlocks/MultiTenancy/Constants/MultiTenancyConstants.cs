namespace VK.Blocks.MultiTenancy.Constants;

/// <summary>
/// Constants used in the MultiTenancy module.
/// </summary>
public static class MultiTenancyConstants
{
    /// <summary>
    /// HTTP header concepts and item keys for multi-tenancy.
    /// </summary>
    public static class Headers
    {
        /// <summary>
        /// The default HTTP header name used to pass the Tenant ID.
        /// </summary>
        public const string TenantId = "X-Tenant-Id";
    }

    /// <summary>
    /// Error constants for multi-tenancy.
    /// </summary>
    public static class Errors
    {
        /// <summary>
        /// Error code for missing tenant.
        /// </summary>
        public const string MissingTenantCode = "MultiTenancy.TenantMissing";

        /// <summary>
        /// Error message for missing tenant.
        /// </summary>
        public const string MissingTenantMessage = "The required TenantId was not provided or could not be resolved.";
    }
}
