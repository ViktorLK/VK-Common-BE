using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Filtering.Internal;

/// <summary>
/// Filter that enforces a token budget on the total number of knowledge tokens.
/// Follows CS.01, CS.03, AP.01.
/// </summary>
internal sealed class TokenBudgetFilter : IVKKnowledgeLifecycleFilter
{
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKKnowledgeSourcingOptions _retrievalOptions;
    private int _accumulatedTokens;

    /// <summary>
    /// Initializes a new instance of <see cref="TokenBudgetFilter"/>.
    /// </summary>
    public TokenBudgetFilter(
        IVKTokenCounter tokenCounter,
        IOptions<VKKnowledgeSourcingOptions> retrievalOptions)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _retrievalOptions = VKGuard.NotNull(retrievalOptions?.Value);
    }

    /// <inheritdoc />
    public Task<VKResult<VKFilterVerdict>> EvaluateAsync(
        VKKnowledgeLifecycleEntry entry,
        VKCorpusContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(entry);
        VKGuard.NotNull(context);

        string content = entry.Knowledge.Segment?.Content ?? string.Empty;
        int tokens = _tokenCounter.CountTokens(content);
        int budget = _retrievalOptions.DefaultTokenBudget ?? 2048;

        if (_accumulatedTokens + tokens > budget)
        {
            return Task.FromResult(VKResult.Success(VKFilterVerdict.Reject));
        }

        _accumulatedTokens += tokens;
        return Task.FromResult(VKResult.Success(VKFilterVerdict.Keep));
    }
}



