namespace VK.Blocks.MultiTenancy.Context.Internal;

/// <summary>
/// Internal keys used for storing multi-tenancy data in HttpContext.Items or other state stores.
/// </summary>
internal static class MultiTenancyContextConstants
{
    /// <summary>
    /// The HttpContext.Items key used to store the source of the tenant resolution (e.g., Header, QueryString, Override).
    /// </summary>
    public const string TenantSource = "VK_MultiTenancy_Source";
}
