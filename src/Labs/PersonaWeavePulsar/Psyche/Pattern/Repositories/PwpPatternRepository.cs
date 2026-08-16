using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Persistence;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Repositories;

/// <summary>
/// Industrial-grade repository implementation for PWP Pattern entity CRUD operations (<see cref="IPwpPatternRepository"/>).
/// </summary>
public sealed class PwpPatternRepository : IPwpPatternRepository
{
    private readonly IVKBaseRepository<PwpPatternEntity> _repository;
    private readonly IVKUnitOfWork<PwpDbContext> _unitOfWork;
    private readonly ILogger<PwpPatternRepository> _logger;

    public PwpPatternRepository(
        IVKBaseRepository<PwpPatternEntity> repository,
        IVKUnitOfWork<PwpDbContext> unitOfWork,
        ILogger<PwpPatternRepository> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<PwpPatternEntity>> GetByIdAsync(VKPatternId patternId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(patternId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == patternId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Failure<PwpPatternEntity>(VKPersistenceErrors.Repository.EntityNotFound);
            }
            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogGetPatternEntityError(ex, patternId.ToString());
            return VKResult.Failure<PwpPatternEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IEnumerable<PwpPatternEntity>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var entities = await _repository.GetListAsync(e => true, cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success<IEnumerable<PwpPatternEntity>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogListPatternEntitiesError(ex);
            return VKResult.Failure<IEnumerable<PwpPatternEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> CreateAsync(PwpPatternEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entity);

        try
        {
            await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogCreatePatternEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(PwpPatternEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entity);

        try
        {
            var existing = await _repository.GetFirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                return VKResult.Failure(VKPersistenceErrors.Repository.EntityNotFound);
            }

            existing.Segment.Content = entity.Segment.Content;
            existing.Segment.Name = entity.Segment.Name;
            existing.Segment.IsEnabled = entity.Segment.IsEnabled;
            existing.Segment.TargetRole = entity.Segment.TargetRole;
            existing.Segment.AbsoluteDepth = entity.Segment.AbsoluteDepth;
            existing.Segment.RelativeAnchor = entity.Segment.RelativeAnchor;
            existing.Segment.Priority = entity.Segment.Priority;

            await _repository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogUpdatePatternEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> DeleteAsync(VKPatternId patternId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(patternId);

        try
        {
            var existing = await _repository.GetFirstOrDefaultAsync(e => e.Id == patternId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                await _repository.DeleteAsync(existing, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogDeletePatternEntityError(ex, patternId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
