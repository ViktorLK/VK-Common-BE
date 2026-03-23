using Microsoft.EntityFrameworkCore;
using VK.Labs.TaskManagement.Layered.Data.Context;
using VK.Labs.TaskManagement.Layered.Data.Entities;
using VK.Labs.TaskManagement.Layered.Data.Repositories.Interfaces;

namespace VK.Labs.TaskManagement.Layered.Data.Repositories.Implementations;

public sealed class ProjectRepository(TaskManagementDbContext context) : IProjectRepository
{
    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Projects
            .Include(p => p.Members)
            .AsNoTracking() // Read-only by default as per Rule 4
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Project>> GetProjectsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Projects
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await context.Projects.AddAsync(project, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        context.Projects.Update(project);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.Projects.Where(p => p.Id == id).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken = default)
    {
        await context.ProjectMembers.AddAsync(member, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
