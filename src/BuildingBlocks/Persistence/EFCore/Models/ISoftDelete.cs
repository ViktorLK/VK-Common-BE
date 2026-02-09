
namespace VK.Blocks.Persistence.EFCore.Models;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
public interface IMultiTenant
{
    string TenantId { get; set; }
}
