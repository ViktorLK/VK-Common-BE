using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VK.Blocks.Validation.Abstractions;
using VK.Blocks.Validation.Options;

namespace VK.Blocks.Validation.Pipeline;

/// <summary>
/// Middleware that can be used for automatic request validation if needed.
/// Currently simpler to use ActionFilters for endpoint-level validation, 
/// but middleware is provided for global concerns.
/// </summary>
public sealed class ValidationMiddleware(
    RequestDelegate next,
    IOptions<ValidationOptions> options)
{
    private readonly ValidationOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context, IValidationPipeline pipeline)
    {
        // Middleware-based validation logic usually requires reading the request body,
        // which might conflict with ModelBinding in Controllers.
        // Usually safer to rely on ActionFilters or MediatR Behaviors.
        
        await next(context);
    }
}
