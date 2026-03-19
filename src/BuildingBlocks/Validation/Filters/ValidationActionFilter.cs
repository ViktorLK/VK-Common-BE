using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VK.Blocks.Validation.Abstractions;
using VK.Blocks.Validation.Attributes;
using VK.Blocks.Validation.Exceptions;

namespace VK.Blocks.Validation.Filters;

/// <summary>
/// Action filter that automatically validates parameters marked with <see cref="ValidateAttribute"/>.
/// </summary>
public sealed class ValidationActionFilter(IValidationPipeline pipeline) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            var hasValidateAttribute = parameter.BindingInfo?.BinderModelName != null;
            
            if (!hasValidateAttribute && parameter is Microsoft.AspNetCore.Mvc.Controllers.ControllerParameterDescriptor controllerParam)
            {
                hasValidateAttribute = controllerParam.ParameterInfo.GetCustomAttributes(typeof(ValidateAttribute), true).Any();
            }

            if (hasValidateAttribute && context.ActionArguments.TryGetValue(parameter.Name, out var argument) && argument != null)
            {
                var result = await pipeline.ValidateAsync(argument);
                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }
            }
        }

        await next();
    }
}
