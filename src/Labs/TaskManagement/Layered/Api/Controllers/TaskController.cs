using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.Authentication.Claims;
using VK.Blocks.Authorization.Features.Permissions;
using VK.Blocks.MultiTenancy.Context;
using VK.Blocks.Validation.Attributes;
using VK.Labs.TaskManagement.Layered.Services.DTOs.Tasks;
using VK.Labs.TaskManagement.Layered.Services.Interfaces;

namespace VK.Labs.TaskManagement.Layered.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TaskController(ITaskService taskService, ITenantContext tenantContext) : ApiControllerBase
{
    private string TenantId => tenantContext.CurrentTenant?.Id ?? "default-tenant";

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await taskService.GetTaskAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetProjectTasks(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await taskService.GetProjectTasksAsync(projectId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [AuthorizePermission("task.write")]
    public async Task<IActionResult> Create([FromBody][Validate] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await taskService.CreateTaskAsync(request, TenantId, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [AuthorizePermission("task.write")]
    public async Task<IActionResult> Update(Guid id, [FromBody][Validate] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id) return BadRequest();

        var result = await taskService.UpdateTaskAsync(request, TenantId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPatch("{id:guid}/status")]
    [AuthorizePermission("task.write")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody][Validate] ChangeTaskStatusRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id) return BadRequest();

        var result = await taskService.ChangeStatusAsync(request, TenantId, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AuthorizePermission("task.write")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await taskService.DeleteTaskAsync(id, TenantId, cancellationToken);
        return HandleResult(result);
    }
}
