using VK.Labs.TaskManagement.Layered.Data.Entities;

namespace VK.Labs.TaskManagement.Layered.Data.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Project>> GetProjectsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken = default);
}
