using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Common.DependencyInjection.Internal;

/// <summary>
/// Partial implementation for AI Defaults feature hooks.
/// Matches the inferred name 'AIDefaults' from VKAIDefaultsOptions.
/// </summary>
internal sealed partial class AIDefaultsFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKAIDefaultsOptions options)
    {
        // Add shared infrastructure here if needed
    }

    /// <summary>Add global validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKAIDefaultsOptions options, List<string> failures)
    {
        if (options.RetryCount < 0)
        {
            failures.Add("Global RetryCount cannot be negative.");
        }
    }
}
