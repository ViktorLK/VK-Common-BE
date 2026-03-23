namespace VK.Labs.TaskManagement.Layered.Services.DTOs.Projects;

public sealed record CreateProjectRequest(string Name, string Description);
public sealed record UpdateProjectRequest(Guid Id, string Name, string Description);
public sealed record AddMemberRequest(Guid ProjectId, Guid UserId, string Role);
public sealed record ProjectResponse(Guid Id, string Name, string Description, IEnumerable<ProjectMemberResponse> Members);
public sealed record ProjectMemberResponse(Guid UserId, string Role);
