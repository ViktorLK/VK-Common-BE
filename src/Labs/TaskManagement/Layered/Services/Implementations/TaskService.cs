using VK.Blocks.Core.Results;
using VK.Labs.TaskManagement.Layered.Data.Entities;
using VK.Labs.TaskManagement.Layered.Data.Repositories.Interfaces;
using VK.Labs.TaskManagement.Layered.Services.Common;
using VK.Labs.TaskManagement.Layered.Services.DTOs.Tasks;
using VK.Labs.TaskManagement.Layered.Services.Interfaces;

namespace VK.Labs.TaskManagement.Layered.Services.Implementations;

public sealed class TaskService(ITaskRepository taskRepository) : ITaskService
{
    public async Task<Result<TaskResponse>> GetTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken);
        if (task == null) 
            return Result.Failure<TaskResponse>(TaskManagementErrors.Tasks.NotFound);

        return Result.Success(new TaskResponse(task.Id, task.Title, task.Description, task.Status, task.ProjectId, task.AssignedToUserId));
    }

    public async Task<Result<IEnumerable<TaskResponse>>> GetProjectTasksAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var tasks = await taskRepository.GetTasksByProjectIdAsync(projectId, cancellationToken);
        var response = tasks.Select(task => new TaskResponse(task.Id, task.Title, task.Description, task.Status, task.ProjectId, task.AssignedToUserId));
        
        return Result.Success(response);
    }

    public async Task<Result<TaskResponse>> CreateTaskAsync(CreateTaskRequest request, string tenantId, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            AssignedToUserId = request.AssignedToUserId,
            Status = "Todo",
            TenantId = tenantId
        };

        await taskRepository.AddAsync(task, cancellationToken);
        return Result.Success(new TaskResponse(task.Id, task.Title, task.Description, task.Status, task.ProjectId, task.AssignedToUserId));
    }

    public async Task<Result> UpdateTaskAsync(UpdateTaskRequest request, string tenantId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task == null)
            return Result.Failure(TaskManagementErrors.Tasks.NotFound);
            
        if (task.TenantId != tenantId)
            return Result.Failure(TaskManagementErrors.Projects.Forbidden);

        task.Title = request.Title;
        task.Description = request.Description;
        task.AssignedToUserId = request.AssignedToUserId;
        await taskRepository.UpdateAsync(task, cancellationToken);
        
        return Result.Success();
    }

    public async Task<Result> ChangeStatusAsync(ChangeTaskStatusRequest request, string tenantId, CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task == null)
            return Result.Failure(TaskManagementErrors.Tasks.NotFound);
            
        if (task.TenantId != tenantId)
            return Result.Failure(TaskManagementErrors.Projects.Forbidden);

        task.Status = request.Status;
        await taskRepository.UpdateAsync(task, cancellationToken);
        
        return Result.Success();
    }

    public async Task<Result> DeleteTaskAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        await taskRepository.DeleteAsync(id, cancellationToken);
        return Result.Success();
    }
}
