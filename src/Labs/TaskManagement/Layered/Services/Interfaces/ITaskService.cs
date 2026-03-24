using VK.Blocks.Core.Results;
using VK.Labs.TaskManagement.Layered.Services.DTOs.Tasks;

namespace VK.Labs.TaskManagement.Layered.Services.Interfaces;

public interface ITaskService
{
    Task<Result<TaskResponse>> GetTaskAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TaskResponse>>> GetProjectTasksAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<TaskResponse>> CreateTaskAsync(CreateTaskRequest request, string tenantId, CancellationToken cancellationToken = default);
    Task<Result> UpdateTaskAsync(UpdateTaskRequest request, string tenantId, CancellationToken cancellationToken = default);
    Task<Result> ChangeStatusAsync(ChangeTaskStatusRequest request, string tenantId, CancellationToken cancellationToken = default);
    Task<Result> DeleteTaskAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
}
