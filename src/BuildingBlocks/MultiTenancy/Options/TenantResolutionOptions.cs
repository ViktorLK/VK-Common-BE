using VK.Blocks.MultiTenancy.Constants;

namespace VK.Blocks.MultiTenancy.Options;

/// <summary>
/// Configuration options for individual tenant resolution strategies.
/// </summary>
public sealed class TenantResolutionOptions
{
    #region Properties

    /// <summary>
    /// Gets or sets the HTTP header name used by the <c>HeaderTenantResolver</c>.
    /// Default is <c>"X-Tenant-Id"</c>.
    /// </summary>
    public string HeaderName { get; set; } = MultiTenancyConstants.Headers.TenantId;

    /// <summary>
    /// Gets or sets the JWT claim type used by the <c>ClaimsTenantResolver</c>.
    /// Default is <c>"tenant_id"</c>.
    /// </summary>
    public string ClaimType { get; set; } = MultiTenancyConstants.Claims.TenantId;

    /// <summary>
    /// Gets or sets the domain template used by the <c>DomainTenantResolver</c>.
    /// Use <c>{tenant}</c> as a placeholder for the tenant segment.
    /// Default is <c>"{tenant}.yourdomain.com"</c>.
    /// </summary>
    public string DomainTemplate { get; set; } = MultiTenancyConstants.Defaults.DomainTemplate;

    /// <summary>
    /// Gets or sets the query string parameter name used by the <c>QueryStringTenantResolver</c>.
    /// Default is <c>"tenantId"</c>.
    /// </summary>
    public string QueryStringParameterName { get; set; } = MultiTenancyConstants.QueryString.TenantIdParameter;

    #endregion
}
