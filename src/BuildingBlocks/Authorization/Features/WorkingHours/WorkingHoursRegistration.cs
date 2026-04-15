using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.WorkingHours.Internal;

namespace VK.Blocks.Authorization.Features.WorkingHours;

/// <summary>
/// Provides extension methods for registering the Working Hours authorization feature.
/// </summary>
internal static class WorkingHoursRegistration
{
    /// <summary>
    /// Adds the Working Hours authorization feature to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddWorkingHoursFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IWorkingHoursProvider, DefaultWorkingHoursProvider>();
        services.TryAddScoped<WorkingHoursAuthorizationHandler>();
        services.TryAddScoped<IWorkingHoursEvaluator>(sp => sp.GetRequiredService<WorkingHoursAuthorizationHandler>());

        return services;
    }
}
