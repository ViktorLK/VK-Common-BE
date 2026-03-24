using FluentValidation;
using VK.Labs.TaskManagement.Layered.Services.DTOs.Tasks;

namespace VK.Labs.TaskManagement.Layered.Services.Validators;

public sealed class ChangeTaskStatusValidator : AbstractValidator<ChangeTaskStatusRequest>
{
    public ChangeTaskStatusValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required.");
            
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => status is "Todo" or "InProgress" or "Done")
            .WithMessage("Status must be one of: Todo, InProgress, Done.");
    }
}
