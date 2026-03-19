using MediatR;
using VK.Blocks.Validation.Abstractions;
using VK.Blocks.Validation.Extensions;

namespace VK.Blocks.Validation.Behaviors;

/// <summary>
/// Pipeline behavior for validating MediatR requests using <see cref="IValidationPipeline"/>.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IValidationPipeline pipeline)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var result = await pipeline.ValidateAsync(request, cancellationToken);
        result.ThrowIfInvalid();

        return await next();
    }
}
