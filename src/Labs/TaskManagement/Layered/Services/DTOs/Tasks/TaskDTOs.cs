namespace VK.Labs.TaskManagement.Layered.Services.DTOs.Tasks;

public sealed record CreateTaskRequest(Guid ProjectId, string Title, string Description, Guid? AssignedToUserId);
public sealed record UpdateTaskRequest(Guid Id, string Title, string Description, Guid? AssignedToUserId);
public sealed record ChangeTaskStatusRequest(Guid Id, string Status);
public sealed record TaskResponse(Guid Id, string Title, string Description, string Status, Guid ProjectId, Guid? AssignedToUserId);
