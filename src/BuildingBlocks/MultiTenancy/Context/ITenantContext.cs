using VK.Blocks.MultiTenancy.Abstractions.Contracts;

namespace VK.Blocks.MultiTenancy.Context;

/// <summary>
/// Provides read-only access to the current request's resolved tenant information.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the resolved tenant information for the current request,
    /// or <c>null</c> if no tenant has been resolved.
    /// </summary>
    TenantInfo? CurrentTenant { get; }

    /// <summary>
    /// Gets a value indicating whether a tenant has been successfully resolved
    /// for the current request.
    /// </summary>
    bool IsResolved { get; }
}
