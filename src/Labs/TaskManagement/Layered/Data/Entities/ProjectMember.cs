namespace VK.Labs.TaskManagement.Layered.Data.Entities;

public sealed class ProjectMember : IMultiTenantEntity
{
    public Guid Id { get; set; }
    
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    public string Role { get; set; } = "Member"; // e.g., Admin, Member, Viewer
    
    public string TenantId { get; set; } = string.Empty;
}
