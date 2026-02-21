using VK.Blocks.Core.Exceptions;
using VK.Blocks.Validation.Abstractions;

namespace VK.Blocks.Validation.Exceptions;

public class ValidationException : BaseException
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(IEnumerable<ValidationError> errors)
        : base("ValidationErrors", "One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }
}
