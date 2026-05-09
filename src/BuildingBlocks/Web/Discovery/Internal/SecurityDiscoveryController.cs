using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Web.Discovery.Internal;

/// <summary>
/// Diagnostic controller that exposes the security topology (Authentication and Authorization metadata).
/// This helps in auditing and frontend discovery.
/// </summary>
[Authorize]
[ApiController]
[Route("api/diagnostics/security")]
internal sealed class SecurityDiscoveryController(
    IEnumerable<IVKSecurityMetadataProvider> providers,
    IOptions<VKSecurityDiscoveryOptions> options) : VKApiController
{
    private readonly IEnumerable<IVKSecurityMetadataProvider> _providers = VKGuard.NotNull(providers);
    private readonly VKSecurityDiscoveryOptions _options = VKGuard.NotNull(options).Value;

    /// <summary>
    /// Gets the unified security topology of the application.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of security topology objects from all registered providers.</returns>
    [HttpGet("topology")]
    public async Task<IActionResult> GetTopology(CancellationToken cancellationToken)
    {
        if (GuardEnabled() is { } guard)
        {
            return guard;
        }

        var tasks = _providers.Select(p => p.GetSecurityTopologyAsync(HttpContext.RequestServices, cancellationToken).AsTask());
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return HandleResult(VKResult<IEnumerable<VKSecurityTopology>>.Success(results));
    }

    /// <summary>
    /// Gets the list of registered security modules.
    /// </summary>
    /// <returns>A list of module names.</returns>
    [HttpGet("modules")]
    public IActionResult GetModules()
    {
        if (GuardEnabled() is { } guard)
        {
            return guard;
        }

        var modules = _providers.Select(p => p.Module).Distinct().ToList();
        return HandleResult(VKResult<IEnumerable<string>>.Success(modules));
    }

    /// <summary>
    /// Guards the endpoint based on <see cref="VKSecurityDiscoveryOptions.Enabled"/> configuration.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> of NotFound if disabled; otherwise <c>null</c>.</returns>
    private NotFoundResult? GuardEnabled()
    {
        return _options.Enabled ? null : NotFound();
    }
}
