using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Structured.Internal;

/// <summary>
/// Default implementation of <see cref="IVKFactCapacityPolicy"/> using <see cref="VKStructuredOptions.MaxFactsPerTenant"/>.
/// Follows AP.03 (internal sealed class in Internal/ folder).
/// </summary>
internal sealed class DefaultFactCapacityPolicy : IVKFactCapacityPolicy
{
    private readonly VKStructuredOptions _options;

    public DefaultFactCapacityPolicy(VKStructuredOptions options)
    {
        _options = VKGuard.NotNull(options);
    }

    public Task<VKResult> ValidateCapacityAsync(VKTenantId? tenantId, int currentFactCount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (currentFactCount >= _options.MaxFactsPerTenant)
        {
            return Task.FromResult(VKResult.Failure(VKStructuredErrors.CapacityExceeded));
        }

        return Task.FromResult(VKResult.Success());
    }
}
