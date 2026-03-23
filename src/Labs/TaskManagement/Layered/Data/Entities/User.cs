namespace VK.Labs.TaskManagement.Layered.Data.Entities;

public sealed class User : IMultiTenantEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    public string TenantId { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];
}
