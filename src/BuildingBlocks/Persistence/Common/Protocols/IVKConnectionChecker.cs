using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Defines a provider-agnostic contract for checking database connectivity.
/// Intended for use by health check integrations (Kubernetes probes, etc.).
/// Does NOT depend on ASP.NET IHealthCheck directly.
/// </summary>
public interface IVKConnectionChecker
{
    /// <summary>
    /// Checks if the database connection is healthy.
    /// </summary>
    // [CS.03]
    Task<VKResult> CheckAsync(CancellationToken cancellationToken = default);
}
