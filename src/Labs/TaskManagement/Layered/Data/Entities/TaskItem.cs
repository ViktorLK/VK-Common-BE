namespace VK.Labs.TaskManagement.Layered.Data.Entities;

public sealed class TaskItem : IMultiTenantEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Todo"; // e.g., Todo, InProgress, Done
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public string TenantId { get; set; } = string.Empty;
}
