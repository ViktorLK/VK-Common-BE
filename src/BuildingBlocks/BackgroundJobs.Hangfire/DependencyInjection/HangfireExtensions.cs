using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.BackgroundJobs.Abstractions;
using VK.Blocks.BackgroundJobs.Hangfire.Services;

namespace VK.Blocks.BackgroundJobs.Hangfire.DependencyInjection;

public static class HangfireExtensions
{
    public static IServiceCollection AddVKHangfireJobs(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var backgroundJobOptions = configuration.GetSection(VK.Blocks.BackgroundJobs.Abstractions.Options.BackgroundJobOptions.SectionName).Get<VK.Blocks.BackgroundJobs.Abstractions.Options.BackgroundJobOptions>() 
            ?? new VK.Blocks.BackgroundJobs.Abstractions.Options.BackgroundJobOptions();

        // Default to MemoryStorage for Labs/Tests
        services.AddHangfire(config => 
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseMemoryStorage()
                  .UseFilter(new AutomaticRetryAttribute { Attempts = backgroundJobOptions.MaxRetryCount });
        });

        // Options
        services.Configure<VK.Blocks.BackgroundJobs.Abstractions.Options.BackgroundJobOptions>(
            configuration.GetSection(VK.Blocks.BackgroundJobs.Abstractions.Options.BackgroundJobOptions.SectionName));

        services.AddHangfireServer();

        services.AddScoped<IBackgroundJobService, HangfireJobService>();
        services.AddScoped<IRecurringJobService, HangfireRecurringJobService>();
        services.AddScoped<HangfireJobRunner>(); // Bridge

        return services;
    }
}
