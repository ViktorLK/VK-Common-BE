using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VK.Blocks.Core;

namespace VK.Blocks.Observability.HealthChecks.Internal;

/// <summary>
/// Default implementation of <see cref="IVKHealthCheckRegistry"/>.
/// </summary>
// [AP.01] sealed
// [AP.03] Internal scoping without VK prefix
internal sealed class DefaultHealthCheckRegistry : IVKHealthCheckRegistry
{
    private readonly object _lock = new();
    private readonly List<Action<IHealthChecksBuilder>> _registrations = [];
    private readonly HashSet<string> _checkNames = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IReadOnlyCollection<string> RegisteredCheckNames
    {
        get
        {
            lock (_lock)
            {
                return new ReadOnlyCollection<string>([.. _checkNames]);
            }
        }
    }

    /// <inheritdoc />
    public IVKHealthCheckRegistry Register<TCheck>(
        string name,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
        where TCheck : class, IHealthCheck
    {
        VKGuard.NotNullOrWhiteSpace(name, nameof(name));

        lock (_lock)
        {
            _checkNames.Add(name);
            _registrations.Add(builder => builder.AddCheck<TCheck>(name, failureStatus, tags, timeout));
        }

        return this;
    }

    /// <inheritdoc />
    public IVKHealthCheckRegistry Register(
        string name,
        Func<IServiceProvider, IHealthCheck> factory,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        VKGuard.NotNullOrWhiteSpace(name, nameof(name));
        VKGuard.NotNull(factory, nameof(factory));

        lock (_lock)
        {
            _checkNames.Add(name);
            _registrations.Add(builder => builder.Add(new HealthCheckRegistration(name, factory, failureStatus, tags, timeout)));
        }

        return this;
    }

    /// <inheritdoc />
    public void ApplyTo(IHealthChecksBuilder builder)
    {
        VKGuard.NotNull(builder, nameof(builder));

        lock (_lock)
        {
            foreach (var registration in _registrations)
            {
                registration(builder);
            }
        }
    }
}
