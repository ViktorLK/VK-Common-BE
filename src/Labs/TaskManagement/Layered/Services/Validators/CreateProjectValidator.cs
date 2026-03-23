using FluentValidation;
using VK.Labs.TaskManagement.Layered.Services.DTOs.Projects;

namespace VK.Labs.TaskManagement.Layered.Services.Validators;

public sealed class CreateProjectValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(100).WithMessage("Project name must not exceed 100 characters.");
            
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Project description must not exceed 500 characters.");
    }
}
