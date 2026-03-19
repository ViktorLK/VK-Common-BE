namespace VK.Blocks.MultiTenancy.Constants;

/// <summary>
/// Constants used across the MultiTenancy module.
/// </summary>
public static class MultiTenancyConstants
{
    /// <summary>
    /// HTTP header constants for multi-tenancy.
    /// </summary>
    public static class Headers
    {
        /// <summary>
        /// The default HTTP header name used to pass the Tenant ID.
        /// </summary>
        public const string TenantId = "X-Tenant-Id";
    }

    /// <summary>
    /// JWT claim type constants for multi-tenancy.
    /// </summary>
    public static class Claims
    {
        /// <summary>
        /// The default JWT claim type for the Tenant ID.
        /// </summary>
        public const string TenantId = "tenant_id";
    }

    /// <summary>
    /// Query string parameter constants for multi-tenancy.
    /// </summary>
    public static class QueryString
    {
        /// <summary>
        /// The default query string parameter name for the Tenant ID.
        /// </summary>
        public const string TenantIdParameter = "tenantId";
    }

    /// <summary>
    /// Default configuration values for multi-tenancy.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// The default domain template for subdomain-based tenant resolution.
        /// </summary>
        public const string DomainTemplate = "{tenant}.yourdomain.com";
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

        /// <summary>
        /// Error code for invalid tenant implementation.
        /// </summary>
        public const string InvalidTenantImplementationCode = "MultiTenancy.InvalidTenantImplementation";

        /// <summary>
        /// Problem details type URL.
        /// </summary>
        public const string ProblemDetailsType = "https://tools.ietf.org/html/rfc7807";

        /// <summary>
        /// Problem details title for resolution failure.
        /// </summary>
        public const string ProblemDetailsTitle = "Tenant Resolution Failed";
    }

    /// <summary>
    /// Configuration key constants.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// The placeholder used in domain templates.
        /// </summary>
        public const string TenantPlaceholder = "{tenant}";
    }
}
