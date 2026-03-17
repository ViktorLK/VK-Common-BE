using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.ExceptionHandling.Abstractions;

namespace VK.Blocks.ExceptionHandling.DependencyInjection;

/// <summary>
/// Defines a builder for configuring exception handling.
/// </summary>
public interface IExceptionHandlingBuilder
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Adds a custom exception handler to the pipeline.
    /// </summary>
    IExceptionHandlingBuilder AddHandler<T>() where T : class, IExceptionHandler;
}

internal sealed class ExceptionHandlingBuilder(IServiceCollection services) : IExceptionHandlingBuilder
{
    public IServiceCollection Services { get; } = services;

    public IExceptionHandlingBuilder AddHandler<T>() where T : class, IExceptionHandler
    {
        Services.AddScoped<IExceptionHandler, T>();
        return this;
    }
}
