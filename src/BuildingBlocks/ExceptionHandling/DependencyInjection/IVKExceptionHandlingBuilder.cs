using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// A builder for configuring the ExceptionHandling building block.
/// Complies with AP.02 (DI Registration Pattern).
/// </summary>
public interface IVKExceptionHandlingBuilder
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Adds an exception-to-error mapper.
    /// </summary>
    /// <typeparam name="TMapper">The type of the mapper.</typeparam>
    /// <returns>The builder for chaining.</returns>
    IVKExceptionHandlingBuilder AddMapper<TMapper>() where TMapper : class;
}

