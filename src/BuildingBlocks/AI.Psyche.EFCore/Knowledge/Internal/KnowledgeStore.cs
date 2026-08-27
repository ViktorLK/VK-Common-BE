using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Knowledge.Internal;

/// <summary>
/// EFCore implementation of Psyche's <see cref="IVKKnowledgeStore"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class KnowledgeStore(
    IVKEntityReadRepository<VKPsycheKnowledgeEntity> repository,
    ILogger<KnowledgeStore> logger) : IVKKnowledgeStore
{
    private readonly IVKEntityReadRepository<VKPsycheKnowledgeEntity> _repository = VKGuard.NotNull(repository);
    private readonly ILogger<KnowledgeStore> _logger = VKGuard.NotNull(logger);

    public async Task<VKResult<IReadOnlyList<VKKnowledgeEntry>>> GetKnowledgeEntriesAsync(
        IReadOnlyList<VKKnowledgeId> knowledgeIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(knowledgeIds);

        if (knowledgeIds.Count == 0)
        {
            return VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>([]);
        }

        try
        {
            var entities = await _repository.GetListAsync(
                e => knowledgeIds.Contains(e.Id),
                include: q => q.Include(e => e.Keys),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var domainList = entities.Select(e => e.ToDomain()).ToList();
            return VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>(domainList);
        }
        catch (Exception ex)
        {
            _logger.LogGetKnowledgeStoreError(ex, string.Join(",", knowledgeIds));
            return VKResult.Failure<IReadOnlyList<VKKnowledgeEntry>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
