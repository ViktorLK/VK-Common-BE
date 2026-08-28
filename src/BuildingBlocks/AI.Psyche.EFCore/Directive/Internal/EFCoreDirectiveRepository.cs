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
/// EF Core persistence adapter implementing both <see cref="IVKDirectiveRepository"/> and <see cref="IVKDirectiveStore"/>.
/// Adapts persistence entity operations using the Source-Generated <see cref="VKPsycheDirectiveMapper"/>.
/// Follows AP.01 (sealed class default) and CS.03 (ConfigureAwait(false)).
/// </summary>
internal sealed class EFCoreDirectiveRepository(
    IVKEntityRepository<VKPsycheDirectiveEntity> repository,
    IVKUnitOfWork unitOfWork,
    ILogger<EFCoreDirectiveRepository> logger) : IVKDirectiveRepository, IVKDirectiveStore
{
    private readonly IVKEntityRepository<VKPsycheDirectiveEntity> _repository = VKGuard.NotNull(repository);
    private readonly IVKUnitOfWork _unitOfWork = VKGuard.NotNull(unitOfWork);
    private readonly ILogger<EFCoreDirectiveRepository> _logger = VKGuard.NotNull(logger);

    // =========================================================================
    // IVKReadRepository<VKDirectiveCharter, VKDirectiveId> Implementation
    // =========================================================================

    public async Task<VKResult<VKDirectiveCharter>> FindByIdAsync(VKDirectiveId id, CancellationToken ct = default)
    {
        if (id == VKDirectiveId.Default)
        {
            return VKResult.Failure<VKDirectiveCharter>(VKDirectiveErrors.NotFound);
        }

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(d => d.Id == id, cancellationToken: ct).ConfigureAwait(false);
            return entity is not null
                ? VKResult.Success(entity.ToDomain())
                : VKResult.Failure<VKDirectiveCharter>(VKDirectiveErrors.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogGetDirectivesStoreError(ex, id.ToString());
            return VKResult.Failure<VKDirectiveCharter>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IReadOnlyList<VKDirectiveCharter>>> ListByIdsAsync(
        IReadOnlyList<VKDirectiveId> ids,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        VKGuard.NotNull(ids);

        if (ids.Count == 0)
        {
            return VKResult.Success<IReadOnlyList<VKDirectiveCharter>>([]);
        }

        try
        {
            var entities = await _repository.GetListAsync(
                predicate: d => ids.Contains(d.Id),
                cancellationToken: ct).ConfigureAwait(false);

            var domainList = entities.Select(e => e.ToDomain()).ToList().AsReadOnly();
            return VKResult.Success<IReadOnlyList<VKDirectiveCharter>>(domainList);
        }
        catch (Exception ex)
        {
            _logger.LogGetDirectivesStoreError(ex, string.Join(",", ids));
            return VKResult.Failure<IReadOnlyList<VKDirectiveCharter>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<bool> ExistsAsync(VKDirectiveId id, CancellationToken ct = default)
    {
        if (id == VKDirectiveId.Default)
        {
            return false;
        }

        return await _repository.AnyAsync(d => d.Id == id, cancellationToken: ct).ConfigureAwait(false);
    }

    // =========================================================================
    // IVKWriteRepository<VKDirectiveCharter, VKDirectiveId> Implementation
    // =========================================================================

    public async Task<VKResult> AddAsync(VKDirectiveCharter item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);

        try
        {
            var entity = item.ToEntity();
            await _repository.AddAsync(entity, ct).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogGetDirectivesStoreError(ex, item.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(VKDirectiveCharter item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);

        try
        {
            var trackResult = await _repository.TrackAndUpdateByIdAsync(
                id: item.Id,
                domain: item,
                mapOntoAction: static (domain, entity) => domain.MapOnto(entity),
                notFoundError: VKDirectiveErrors.NotFound,
                ct: ct).ConfigureAwait(false);

            if (trackResult.IsFailure)
            {
                return trackResult;
            }

            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogGetDirectivesStoreError(ex, item.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> DeleteAsync(VKDirectiveId id, CancellationToken ct = default)
    {
        try
        {
            var entity = await _repository.GetTrackedFirstOrDefaultAsync(d => d.Id == id, cancellationToken: ct).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Failure(VKDirectiveErrors.NotFound);
            }

            await _repository.DeleteAsync(entity, ct).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogGetDirectivesStoreError(ex, id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    // =========================================================================
    // IVKDirectiveStore Backward Compatibility
    // =========================================================================

    public Task<VKResult<IReadOnlyList<VKDirectiveCharter>>> GetDirectivesAsync(
        IReadOnlyList<VKDirectiveId> directiveIds,
        CancellationToken cancellationToken = default)
    {
        return ListByIdsAsync(directiveIds, cancellationToken);
    }
}
