using VK.Blocks.Core.Results;
using VK.Labs.TaskManagement.Layered.Data.Entities;
using VK.Labs.TaskManagement.Layered.Data.Repositories.Interfaces;
using VK.Labs.TaskManagement.Layered.Services.Common;
using VK.Labs.TaskManagement.Layered.Services.DTOs.Projects;
using VK.Labs.TaskManagement.Layered.Services.Interfaces;

namespace VK.Labs.TaskManagement.Layered.Services.Implementations;

public sealed class ProjectService(IProjectRepository projectRepository) : IProjectService
{
    public async Task<Result<ProjectResponse>> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null) 
            return Result.Failure<ProjectResponse>(TaskManagementErrors.Projects.NotFound);

        return Result.Success(new ProjectResponse(
            project.Id, 
            project.Name, 
            project.Description, 
            project.Members.Select(m => new ProjectMemberResponse(m.UserId, m.Role))));
    }

    public async Task<Result<IEnumerable<ProjectResponse>>> GetUserProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var projects = await projectRepository.GetProjectsForUserAsync(userId, cancellationToken);
        var response = projects.Select(p => new ProjectResponse(
            p.Id, 
            p.Name, 
            p.Description, 
            p.Members.Select(m => new ProjectMemberResponse(m.UserId, m.Role))));
            
        return Result.Success(response);
    }

    public async Task<Result<ProjectResponse>> CreateProjectAsync(CreateProjectRequest request, string tenantId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            TenantId = tenantId
        };

        var member = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = currentUserId,
            Role = "Admin",
            TenantId = tenantId
        };

        project.Members.Add(member);
        await projectRepository.AddAsync(project, cancellationToken);

        return Result.Success(new ProjectResponse(project.Id, project.Name, project.Description, [new ProjectMemberResponse(currentUserId, "Admin")]));
    }

    public async Task<Result> UpdateProjectAsync(UpdateProjectRequest request, string tenantId, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project == null)
            return Result.Failure(TaskManagementErrors.Projects.NotFound);
            
        if (project.TenantId != tenantId)
            return Result.Failure(TaskManagementErrors.Projects.Forbidden);

        project.Name = request.Name;
        project.Description = request.Description;
        await projectRepository.UpdateAsync(project, cancellationToken);
        
        return Result.Success();
    }

    public async Task<Result> DeleteProjectAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        // Simple delete; repository handles query filters usually but safe check here if needed
        await projectRepository.DeleteAsync(id, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> AddMemberAsync(AddMemberRequest request, string tenantId, CancellationToken cancellationToken = default)
    {
        var member = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            UserId = request.UserId,
            Role = request.Role,
            TenantId = tenantId
        };
        await projectRepository.AddMemberAsync(member, cancellationToken);
        return Result.Success();
    }
}
