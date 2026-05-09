namespace VK.Blocks.Web.Internal;

/// <summary>
/// Constants used internally by the Web building block.
/// </summary>
public static class WebConstants
{
    /// <summary>
    /// Constants for HttpContext Items keys.
    /// </summary>
    public static class Items
    {
        /// <summary>
        /// Key for storing identified Tenant ID in HttpContext.Items.
        /// </summary>
        public const string TenantId = "VK_TenantId";

        /// <summary>
        /// Key for storing identified Correlation ID in HttpContext.Items.
        /// </summary>
        public const string CorrelationId = "VK_CorrelationId";
    }
}
