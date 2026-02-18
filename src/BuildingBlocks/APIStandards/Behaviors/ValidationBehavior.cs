using FluentValidation;
using MediatR;
using VK.Blocks.APIStandards.Caches;
using VK.Blocks.APIStandards.Shared;

namespace VK.Blocks.APIStandards.Behaviors;

/// <summary>
/// Pipeline behavior for validating requests using FluentValidation.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
/// </remarks>
/// <param name="validators">The validators for the request.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    #region Public Methods

    /// <inheritdoc />
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

        var validationFailures = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        // Rationale: Aggregates validation errors from all validators, maps them to the application's Error type, and eliminates duplicates.
        var errors = validationFailures
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .Select(validationFailure => new Error(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage,
                ErrorType.Validation))
            .Distinct()
            .ToList();

        if (errors.Count == 0)
        {
            return await next();
        }

        // Must use strict equality check
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(errors);
        }

        if (typeof(TResponse).IsGenericType)
        {
            var genericTypeDef = typeof(TResponse).GetGenericTypeDefinition();
            if (genericTypeDef == typeof(Result<>) ||
                genericTypeDef.IsAssignableFrom(typeof(Result<>)))
            {
                var resultType = typeof(TResponse).GetGenericArguments()[0];
                var factory = ValidationFailureCache.GetOrAdd(resultType);
                return (TResponse)factory(errors);
            }
        }

        if (typeof(Result).IsAssignableFrom(typeof(TResponse)))
        {
            throw new InvalidOperationException($"The response type {typeof(TResponse).Name} inherits from Result but is not handled.");
        }

        throw new ValidationException(errors.Select(e => new FluentValidation.Results.ValidationFailure(e.Code, e.Description)));
    }

    #endregion
}
