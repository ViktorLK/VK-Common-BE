using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Health check implementation that monitors circuit breaker states.
/// Follows [AP.01], [CS.01].
/// </summary>
public sealed class VKResilienceHealthCheck : IHealthCheck
{
    private readonly IVKCircuitBreaker _circuitBreaker;

    public VKResilienceHealthCheck(IVKCircuitBreaker circuitBreaker)
    {
        _circuitBreaker = VKGuard.NotNull(circuitBreaker);
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var openKeys = _circuitBreaker.GetKeysOnCooldown();

        if (openKeys.Count > 0)
        {
            var data = new Dictionary<string, object>
            {
                { "OpenCircuitBreakers", openKeys },
                { "Count", openKeys.Count }
            };

            return Task.FromResult(HealthCheckResult.Degraded(
                $"Resilience Warning: {openKeys.Count} circuit breaker(s) currently open: [{string.Join(", ", openKeys)}]",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy("All resilience circuit breakers are closed and operational."));
    }
}
