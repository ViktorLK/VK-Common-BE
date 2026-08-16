using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Persistence;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Directive.Repositories;

/// <summary>
/// Industrial-grade repository implementation for PWP Directive entity CRUD operations (<see cref="IPwpDirectiveRepository"/>).
/// </summary>
public sealed class PwpDirectiveRepository : IPwpDirectiveRepository
{
    private readonly IVKBaseRepository<PwpDirectiveEntity> _repository;
    private readonly IVKUnitOfWork<PwpDbContext> _unitOfWork;
    private readonly ILogger<PwpDirectiveRepository> _logger;

    public PwpDirectiveRepository(
        IVKBaseRepository<PwpDirectiveEntity> repository,
        IVKUnitOfWork<PwpDbContext> unitOfWork,
        ILogger<PwpDirectiveRepository> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<PwpDirectiveEntity>> GetByIdAsync(VKDirectiveId directiveId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(directiveId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == directiveId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Failure<PwpDirectiveEntity>(VKPersistenceErrors.Repository.EntityNotFound);
            }

            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogGetDirectiveEntityError(ex, directiveId.ToString());
            return VKResult.Failure<PwpDirectiveEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IEnumerable<PwpDirectiveEntity>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var list = await _repository.GetListAsync(e => true, cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success<IEnumerable<PwpDirectiveEntity>>(list);
        }
        catch (Exception ex)
        {
            _logger.LogListDirectiveEntitiesError(ex);
            return VKResult.Failure<IEnumerable<PwpDirectiveEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> CreateAsync(PwpDirectiveEntity entity, CancellationToken cancellationToken = default)
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
            _logger.LogCreateDirectiveEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(PwpDirectiveEntity entity, CancellationToken cancellationToken = default)
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

            existing.BehaviorRules = entity.BehaviorRules;
            existing.SafetyRules = entity.SafetyRules;
            existing.OutputConstraints = entity.OutputConstraints;
            existing.Overview = entity.Overview;

            await _repository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogUpdateDirectiveEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> DeleteAsync(VKDirectiveId directiveId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(directiveId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == directiveId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is not null)
            {
                await _repository.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogDeleteDirectiveEntityError(ex, directiveId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
