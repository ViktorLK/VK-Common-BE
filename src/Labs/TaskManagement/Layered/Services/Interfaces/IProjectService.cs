using VK.Blocks.Core.Results;
using VK.Labs.TaskManagement.Layered.Services.DTOs.Projects;

namespace VK.Labs.TaskManagement.Layered.Services.Interfaces;

public interface IProjectService
{
    Task<Result<ProjectResponse>> GetProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ProjectResponse>>> GetUserProjectsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<ProjectResponse>> CreateProjectAsync(CreateProjectRequest request, string tenantId, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<Result> UpdateProjectAsync(UpdateProjectRequest request, string tenantId, CancellationToken cancellationToken = default);
    Task<Result> DeleteProjectAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<Result> AddMemberAsync(AddMemberRequest request, string tenantId, CancellationToken cancellationToken = default);
}
