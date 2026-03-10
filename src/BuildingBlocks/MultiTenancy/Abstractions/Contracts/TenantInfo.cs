namespace VK.Blocks.MultiTenancy.Abstractions.Contracts;

/// <summary>
/// Represents the core information about a tenant in a multi-tenant system.
/// </summary>
/// <param name="Id">The unique identifier of the tenant.</param>
/// <param name="Name">The display name of the tenant.</param>
/// <param name="ConnectionString">The optional connection string specific to this tenant's data store.</param>
public sealed record TenantInfo(string Id, string Name, string? ConnectionString = null);
