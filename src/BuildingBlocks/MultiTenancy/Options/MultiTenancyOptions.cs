namespace VK.Blocks.MultiTenancy.Options;

/// <summary>
/// Top-level configuration options for the multi-tenancy module.
/// </summary>
public sealed class MultiTenancyOptions
{
    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether tenant resolution is mandatory.
    /// When set to <c>true</c>, requests without a resolved tenant will receive a 401 response.
    /// Default is <c>true</c>.
    /// </summary>
    public bool EnforceTenancy { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of enabled resolver types.
    /// When empty, all registered resolvers will be used.
    /// </summary>
    public List<TenantResolverType> EnabledResolvers { get; set; } = [];

    #endregion
}
