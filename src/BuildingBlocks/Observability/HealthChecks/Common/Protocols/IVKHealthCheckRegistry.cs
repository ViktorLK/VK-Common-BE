using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VK.Blocks.Observability;

/// <summary>
/// Aggregates health checks across modules into a unified contract.
/// Complies with Manifest §11.
/// </summary>
// [AP.03] Level 1 Public API
public interface IVKHealthCheckRegistry
{
    /// <summary>
    /// Registers a health check of type <typeparamref name="TCheck"/>.
    /// </summary>
    IVKHealthCheckRegistry Register<TCheck>(
        string name,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
        where TCheck : class, IHealthCheck;

    /// <summary>
    /// Registers a health check using an instantiation factory.
    /// </summary>
    IVKHealthCheckRegistry Register(
        string name,
        Func<IServiceProvider, IHealthCheck> factory,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null);

    /// <summary>
    /// Gets the names of all registered health checks.
    /// </summary>
    IReadOnlyCollection<string> RegisteredCheckNames { get; }

    /// <summary>
    /// Applies the registered health checks to the specified <see cref="IHealthChecksBuilder"/>.
    /// </summary>
    void ApplyTo(IHealthChecksBuilder builder);
}
