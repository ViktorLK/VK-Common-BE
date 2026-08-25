using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Session.Internal;

/// <summary>
/// Industrial-grade repository implementation for pure Psyche Session entity CRUD operations.
/// </summary>
internal sealed class SessionRepository : IVKPsycheSessionRepository
{
    private readonly IVKBaseRepository<VKPsycheSessionEntity> _repository;
    private readonly IVKUnitOfWork _unitOfWork;
    private readonly ILogger<SessionRepository> _logger;

    public SessionRepository(
        IVKBaseRepository<VKPsycheSessionEntity> repository,
        IVKUnitOfWork unitOfWork,
        ILogger<SessionRepository> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKPsycheSessionEntity>> GetByIdAsync(VKSessionId sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(sessionId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == sessionId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Failure<VKPsycheSessionEntity>(VKPersistenceErrors.Repository.EntityNotFound);
            }
            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogGetSessionEntityError(ex, sessionId.ToString());
            return VKResult.Failure<VKPsycheSessionEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IEnumerable<VKPsycheSessionEntity>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var entities = await _repository.GetListAsync(e => true, cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success<IEnumerable<VKPsycheSessionEntity>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogListSessionEntitiesError(ex);
            return VKResult.Failure<IEnumerable<VKPsycheSessionEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> CreateAsync(VKPsycheSessionEntity entity, CancellationToken cancellationToken = default)
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
            _logger.LogCreateSessionEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(VKPsycheSessionEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entity);

        try
        {
            var existing = await _repository.GetFirstOrDefaultAsync(s => s.Id == entity.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                return VKResult.Failure(VKPersistenceErrors.Repository.EntityNotFound);
            }

            existing.Mode = entity.Mode;
            existing.ParentSessionId = entity.ParentSessionId;
            existing.ForkSourceSessionId = entity.ForkSourceSessionId;
            existing.ForkPointRef = entity.ForkPointRef;
            existing.Status = entity.Status;
            existing.TurnCount = entity.TurnCount;
            existing.UpdatedAt = entity.UpdatedAt;
            existing.LastActivityAt = entity.LastActivityAt;
            existing.KnowledgeStateJson = entity.KnowledgeStateJson;

            await _repository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogUpdateSessionEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> DeleteAsync(VKSessionId sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(sessionId);

        try
        {
            var existing = await _repository.GetFirstOrDefaultAsync(e => e.Id == sessionId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                await _repository.DeleteAsync(existing, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogDeleteSessionEntityError(ex, sessionId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
