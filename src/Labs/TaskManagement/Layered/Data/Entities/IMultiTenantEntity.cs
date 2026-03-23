namespace VK.Labs.TaskManagement.Layered.Data.Entities;

/// <summary>
/// Dummy interface for IMultiTenantEntity to satisfy compilation if not referencing the actual Base library.
/// In a real scenario, this would come from VK.Blocks.Core or VK.Blocks.MultiTenancy.
/// </summary>
public interface IMultiTenantEntity
{
    string TenantId { get; set; }
}
