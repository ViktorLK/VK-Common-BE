using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Observability.HealthChecks.Internal;
using VK.Blocks.Observability.Metrics.Internal;
using VK.Blocks.Observability.Redaction.Internal;
using VK.Blocks.Observability.Tracing.Internal;

namespace VK.Blocks.Observability;

/// <summary>
/// A marker type for the VK.Blocks.Observability building block.
/// Complies with BB.02, BB.03, and AP.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution; contains no executable logic.")]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKObservabilityBlock
{
    // [BB.03] Custom Hook for block-level service bindings
    // [AP.02] TryAdd idempotent registration policy
    static partial void RegisterBlockCustom(IVKObservabilityBuilder builder)
    {
        var services = builder.Services;

        // Logging Enrichers
        services.TryAddEnumerable(ServiceDescriptor.Transient<IVKLogEnricher, VKApplicationEnricher>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IVKLogEnricher, VKUserContextEnricher>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IVKLogEnricher, VKTraceContextEnricher>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IVKLogEnricher, VKTenantContextEnricher>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IVKLogEnricher, VKCorrelationIdEnricher>());
        services.TryAddTransient<IVKLogContextEnricher, VKActivityLogContextEnricher>();

        // Metrics Factory
        services.TryAddSingleton<IVKMetricFactory, DefaultMetricFactory>();

        // Tracing Scope
        services.TryAddSingleton<IVKActivityScope, DefaultActivityScope>();

        // Telemetry Redactor
        services.TryAddSingleton<IVKTelemetryRedactor, DefaultTelemetryRedactor>();

        // Health Check Registry
        services.TryAddSingleton<IVKHealthCheckRegistry, DefaultHealthCheckRegistry>();
    }
}
