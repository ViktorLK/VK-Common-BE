using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.Ingesting.Internal;

/// <summary>
/// In-memory implementation of <see cref="IVKKnowledgeHistoryStore"/>.
/// Follows CS.01, AP.01.
/// </summary>
internal sealed class InMemoryKnowledgeHistoryStore : IVKKnowledgeHistoryStore
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<VKKnowledgeVersion>> _versions = new();

    /// <inheritdoc />
    public Task<VKResult> SaveVersionAsync(VKKnowledgeVersion version, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(version);

        var bag = _versions.GetOrAdd(version.DocumentId, _ => new ConcurrentBag<VKKnowledgeVersion>());
        bag.Add(version);

        return Task.FromResult(VKResult.Success());
    }

    /// <inheritdoc />
    public Task<VKResult<IReadOnlyList<VKKnowledgeVersion>>> GetVersionsAsync(string documentId, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(documentId);

        if (_versions.TryGetValue(documentId, out var bag))
        {
            var ordered = bag.OrderBy(v => v.Version).ToList();
            return Task.FromResult(VKResult.Success<IReadOnlyList<VKKnowledgeVersion>>(ordered));
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKKnowledgeVersion>>([]));
    }

    /// <inheritdoc />
    public Task<VKResult<VKKnowledgeVersion>> GetVersionAsync(string documentId, int version, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(documentId);

        if (_versions.TryGetValue(documentId, out var bag))
        {
            var target = bag.FirstOrDefault(v => v.Version == version);
            if (target is not null)
            {
                return Task.FromResult(VKResult.Success(target));
            }
        }

        return Task.FromResult(VKResult.Failure<VKKnowledgeVersion>(
            VKError.NotFound("AI.Corpus.History.NotFound", $"Version {version} for document {documentId} was not found."))); // [CS.01]
    }
}
