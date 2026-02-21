using FluentValidation;
using MediatR;

namespace VK.Blocks.Validation.Behaviors;

/// <summary>
/// A pipeline behavior that validates the request using <see cref="IValidator{T}"/>.
/// It throws a <see cref="VKValidationException"/> if validation fails.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .Select(f => new Abstractions.ValidationError(f.PropertyName, f.ErrorMessage, f.ErrorCode))
            .ToList();

        if (failures.Count > 0)
        {
            throw new Exceptions.ValidationException(failures);
        }

        return await next();
    }
}
