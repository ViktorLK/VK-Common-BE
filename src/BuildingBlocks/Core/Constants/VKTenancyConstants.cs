namespace VK.Blocks.Core.Constants;

/// <summary>
/// Constants for multi-tenant identification and orchestration.
/// Shared across all BuildingBlocks to ensure architectural consistency (Rule 14).
/// </summary>
public static class VKTenancyConstants
{
    /// <summary>
    /// The standard HTTP header name used to identify the current tenant.
    /// </summary>
    public const string TenantIdHeaderName = "X-Tenant-Id";

    /// <summary>
    /// The standard query string parameter name used to identify the current tenant.
    /// </summary>
    public const string TenantIdQueryParameterName = "tenantId";

    /// <summary>
    /// Standard error codes related to tenancy.
    /// </summary>
    public static class Errors
    {
        public const string TenantMissing = "Tenancy.TenantMissing";
        public const string TenantInvalid = "Tenancy.TenantInvalid";
    }
}
