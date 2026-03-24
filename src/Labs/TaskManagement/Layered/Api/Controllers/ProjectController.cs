using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VK.Blocks.Authentication.Claims;
using VK.Blocks.Authorization.Features.Permissions;
using VK.Blocks.MultiTenancy.Context;
using VK.Blocks.Validation.Attributes;
using VK.Labs.TaskManagement.Layered.Services.DTOs.Projects;
using VK.Labs.TaskManagement.Layered.Services.Interfaces;

namespace VK.Labs.TaskManagement.Layered.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProjectController(IProjectService projectService, ITenantContext tenantContext) : ApiControllerBase
{
    private string TenantId => tenantContext.CurrentTenant?.Id ?? "default-tenant";
    
    private Guid CurrentUserId => Guid.TryParse(
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(VKClaimTypes.UserId)?.Value, 
        out var id) ? id : Guid.Empty;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await projectService.GetProjectAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserProjects(Guid userId, CancellationToken cancellationToken)
    {
        var result = await projectService.GetUserProjectsAsync(userId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [AuthorizePermission("project.write")]
    public async Task<IActionResult> Create([FromBody][Validate] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await projectService.CreateProjectAsync(request, TenantId, CurrentUserId, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
        }
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [AuthorizePermission("project.write")]
    public async Task<IActionResult> Update(Guid id, [FromBody][Validate] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id) return BadRequest();

        var result = await projectService.UpdateProjectAsync(request, TenantId, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [AuthorizePermission("project.write")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await projectService.DeleteProjectAsync(id, TenantId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("members")]
    [AuthorizePermission("project.write")]
    public async Task<IActionResult> AddMember([FromBody][Validate] AddMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await projectService.AddMemberAsync(request, TenantId, cancellationToken);
        return HandleResult(result);
    }
}
