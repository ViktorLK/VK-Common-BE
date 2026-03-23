using Microsoft.EntityFrameworkCore;
using VK.Labs.TaskManagement.Layered.Data.Context;
using VK.Labs.TaskManagement.Layered.Data.Entities;
using VK.Labs.TaskManagement.Layered.Data.Repositories.Interfaces;

namespace VK.Labs.TaskManagement.Layered.Data.Repositories.Implementations;

public sealed class TaskRepository(TaskManagementDbContext context) : ITaskRepository
{
    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<TaskItem>> GetTasksByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await context.Tasks
            .Where(t => t.ProjectId == projectId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await context.Tasks.AddAsync(task, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Update(task);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.Tasks.Where(t => t.Id == id).ExecuteDeleteAsync(cancellationToken);
    }
}
