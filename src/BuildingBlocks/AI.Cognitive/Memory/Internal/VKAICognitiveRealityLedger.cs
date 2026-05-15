using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// A fallback implementation of <see cref="IVKRealityLedger"/> that logs actions.
/// Real implementations would typically use SQL or a document store.
/// </summary>
internal sealed class VKAICognitiveRealityLedger : IVKRealityLedger
{
    private readonly ILogger<VKAICognitiveRealityLedger> _logger;

    public VKAICognitiveRealityLedger(ILogger<VKAICognitiveRealityLedger> logger)
    {
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public Task<VKResult> RecordAsync(string key, object value, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNull(value);

        // In a real scenario, this would persist to a relational database.
        _logger.FactArchived(key);

        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public Task<VKResult<T?>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        _logger.LedgerNotImplemented(key);

        return Task.FromResult(VKResult.Success(default(T)));
    }
}
