using FluentValidation;
using VK.Labs.TaskManagement.Layered.Services.DTOs.Tasks;

namespace VK.Labs.TaskManagement.Layered.Services.Validators;

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");
            
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");
            
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Task description must not exceed 2000 characters.");
    }
}
