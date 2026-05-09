namespace VK.Blocks.MultiTenancy.Context.Internal;

/// <summary>
/// Internal contract for setting the current tenant context.
/// This should only be used by resolution middleware or test infrastructure.
/// </summary>
internal interface IVKTenantContextSetter
{
    /// <summary>
    /// Sets the resolved tenant information for the current scope.
    /// </summary>
    /// <param name="tenantInfo">The resolved tenant information.</param>
    void SetTenant(IVKTenantInfo tenantInfo);
}
