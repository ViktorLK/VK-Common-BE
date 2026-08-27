using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Directive.Internal;

/// <summary>
/// EFCore implementation of Psyche's <see cref="IVKDirectiveStore"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class DirectiveStore(
    IVKEntityReadRepository<VKPsycheDirectiveEntity> repository,
    ILogger<DirectiveStore> logger) : IVKDirectiveStore
{
    private readonly IVKEntityReadRepository<VKPsycheDirectiveEntity> _repository = VKGuard.NotNull(repository);
    private readonly ILogger<DirectiveStore> _logger = VKGuard.NotNull(logger);

    public async Task<VKResult<IReadOnlyList<VKDirectiveCharter>>> GetDirectivesAsync(
        IReadOnlyList<VKDirectiveId> directiveIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(directiveIds);

        if (directiveIds.Count == 0)
        {
            return VKResult.Success<IReadOnlyList<VKDirectiveCharter>>([]);
        }

        try
        {
            var entities = await _repository.GetListAsync(
                e => directiveIds.Contains(e.Id),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var domainList = entities.Select(e => e.ToDomain()).ToList();
            return VKResult.Success<IReadOnlyList<VKDirectiveCharter>>(domainList);
        }
        catch (Exception ex)
        {
            _logger.LogGetDirectivesStoreError(ex, string.Join(",", directiveIds));
            return VKResult.Failure<IReadOnlyList<VKDirectiveCharter>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
