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

        var failures = new List<Abstractions.ValidationError>();
        foreach (var result in validationResults)
        {
            if (result.IsValid)
                continue;

            foreach (var error in result.Errors)
            {
                failures.Add(new Abstractions.ValidationError(
                    error.PropertyName,
                    error.ErrorMessage,
                    error.ErrorCode));
            }
        }

        if (failures.Count > 0)
        {
            throw new Exceptions.ValidationException(failures);
        }

        return await next();
    }
}
