using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

/// <summary>
/// In-memory implementation of <see cref="IVKPatternStore"/> for testing or basic scenarios.
/// </summary>
internal sealed class InMemoryPatternStore : IVKPatternStore
{
    private readonly List<VKPatternEntry> _patterns = new();

    public InMemoryPatternStore()
    {
    }

    public InMemoryPatternStore(IEnumerable<VKPatternEntry> patterns)
    {
        _patterns.AddRange(VKGuard.NotNull(patterns));
    }

    public Task<VKResult<IEnumerable<VKPatternEntry>>> GetCurrentPatternsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(VKResult.Success<IEnumerable<VKPatternEntry>>(_patterns));
    }
}
