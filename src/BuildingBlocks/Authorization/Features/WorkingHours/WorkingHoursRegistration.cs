using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.WorkingHours.Internal;

namespace VK.Blocks.Authorization.Features.WorkingHours;

internal static class WorkingHoursRegistration
{
    public static IServiceCollection AddWorkingHoursFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IWorkingHoursProvider, DefaultWorkingHoursProvider>();
        services.TryAddScoped<WorkingHoursAuthorizationHandler>();
        services.TryAddScoped<IWorkingHoursEvaluator>(sp => sp.GetRequiredService<WorkingHoursAuthorizationHandler>());
        
        return services;
    }
}
