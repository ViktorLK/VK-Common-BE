namespace VK.Blocks.Core.Primitives;

public interface IMultiTenantEntity : IMultiTenant
{
    new string? TenantId { get; set; }
}
