using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Core Presence feature registration and validation.
/// Follows BB.03 and BB.06.
/// </summary>
internal sealed partial class PresenceFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKPresenceOptions options)
    {
        // [BB.03] Register high-precision token meter abstraction (Option B local approximation)
        services.TryAddSingleton<IVKTokenMeter, Common.Internal.LocalTokenMeter>();

        // [BB.03] Register situational presence state resumption store (In-memory fallback)
        services.TryAddSingleton<IVKPresenceStateStore, InMemoryPresenceStateStore>();

        // Register default constitution and quota providers as fallbacks
        services.TryAddSingleton<IVKConstitutionProvider, DefaultConstitutionProvider>();
        services.TryAddSingleton<IVKPresenceQuotaProvider, DefaultPresenceQuotaProvider>();

        // Register new robust security and telemetry providers as fallbacks
        services.TryAddSingleton<IVKPresenceRateLimiter, InMemoryRateLimiter>();
        services.TryAddSingleton<IVKPresenceBalanceAuditor, DefaultBalanceAuditor>();
        services.TryAddSingleton<IVKSystemTelemetry, DefaultSystemTelemetry>();
        services.TryAddSingleton<IVKTenantContextAccessor, DefaultTenantContextAccessor>();

        // Register the in-memory core presence tracker
        services.TryAddSingleton<IVKPresenceTracker, PresenceTracker>();

        // Register the presence assembler to compose multi-dimensional prompt tapestries
        services.TryAddScoped<IVKPresenceAssembler, PresenceAssembler>();

        // Register the local memory eviction channel dispatcher and reader
        var dispatcher = new LocalMemoryEvictionDispatcher();
        services.TryAddSingleton<IVKMemoryEvictionDispatcher>(dispatcher);
        services.TryAddSingleton(dispatcher.Reader);

        // Register the core presence pipeline interceptors (Early Governance and Late Context)
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKCognitivePipelineInterceptor, PresenceGovernanceInterceptor>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKCognitivePipelineInterceptor, PresenceContextInterceptor>());
    }

    // [SG Hook]
    static partial void ValidateCustom(VKPresenceOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.DefaultTokenLimit <= 0)
        {
            failures.Add("VKPresenceOptions.DefaultTokenLimit must be greater than zero.");
        }

        if (options.TruncationThreshold <= 0.0f || options.TruncationThreshold > 1.0f)
        {
            failures.Add("VKPresenceOptions.TruncationThreshold must be between 0.0 and 1.0.");
        }
    }
}
