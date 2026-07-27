using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Engram.Reclamation.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Reclamation feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), OptionsType = typeof(VKReclamationOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class ReclamationFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKReclamationOptions options)
    {
        services.TryAddSingleton<IVKDecayStrategy, DefaultDecayStrategy>();
        services.TryAddSingleton<IVKPruningStrategy, DefaultPruningStrategy>();
        services.TryAddScoped<IVKMemoryReclamationService, DefaultMemoryReclamationService>();

        // Background worker for periodic memory reclamation
        services.AddHostedService<ReclamationBackgroundWorker>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKReclamationOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.ReclamationIntervalMinutes <= 0)
        {
            failures.Add("VKReclamationOptions.ReclamationIntervalMinutes must be greater than zero.");
        }

        if (options.ReclamationBatchSize <= 0)
        {
            failures.Add("VKReclamationOptions.ReclamationBatchSize must be greater than zero.");
        }

        if (options.L1HalfLifeHours <= 0)
        {
            failures.Add("VKReclamationOptions.L1HalfLifeHours must be greater than zero.");
        }

        if (options.L2HalfLifeHours <= 0)
        {
            failures.Add("VKReclamationOptions.L2HalfLifeHours must be greater than zero.");
        }

        if (options.L3HalfLifeHours <= 0)
        {
            failures.Add("VKReclamationOptions.L3HalfLifeHours must be greater than zero.");
        }
    }
}
