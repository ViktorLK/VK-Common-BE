namespace VK.Blocks.Core;

/// <summary>
/// Provides mechanism to set the current tenant context for scoped background operations. (AP.01)
/// </summary>
public interface IVKTenantSetter
{
    /// <summary>
    /// Sets the current tenant identifier for the active scope.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to activate.</param>
    void SetCurrentTenantId(VKTenantId tenantId);
}
