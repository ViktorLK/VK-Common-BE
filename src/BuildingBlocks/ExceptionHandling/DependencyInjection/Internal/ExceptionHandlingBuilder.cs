using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.ExceptionHandling.DependencyInjection.Internal;

/// <summary>
/// Implementation of the exception handling builder.
/// </summary>
internal sealed class ExceptionHandlingBuilder(IServiceCollection services) : IVKExceptionHandlingBuilder
{
    public IServiceCollection Services { get; } = services;

    public IVKExceptionHandlingBuilder AddMapper<TMapper>() where TMapper : class
    {
        // AP.02: Use TryAdd pattern for individual services
        Services.TryAddTransient<TMapper>();

        return this;
    }
}

