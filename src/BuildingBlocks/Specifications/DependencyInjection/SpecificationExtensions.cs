using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Specifications.Abstractions;
using VK.Blocks.Specifications.Evaluators;

namespace VK.Blocks.Specifications.DependencyInjection;

/// <summary>
/// Dependency injection extensions for specifications.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Adds specification services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSpecifications(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ISpecificationEvaluator<>), typeof(SpecificationEvaluator<>));
        return services;
    }
}
