using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Persistence;
using VK.Labs.PersonaWeavePulsar.Psyche.Session.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Session.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Session.Repositories;

/// <summary>
/// Industrial-grade repository implementation for PWP Session entity CRUD operations (<see cref="IPwpSessionRepository"/>).
/// </summary>
public sealed class PwpSessionRepository : IPwpSessionRepository
{
    private readonly IVKBaseRepository<PwpSessionEntity> _repository;
    private readonly IVKUnitOfWork<PwpDbContext> _unitOfWork;
    private readonly ILogger<PwpSessionRepository> _logger;

    public PwpSessionRepository(
        IVKBaseRepository<PwpSessionEntity> repository,
        IVKUnitOfWork<PwpDbContext> unitOfWork,
        ILogger<PwpSessionRepository> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<PwpSessionEntity>> GetByIdAsync(VKSessionId sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(sessionId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == sessionId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Failure<PwpSessionEntity>(VKPersistenceErrors.Repository.EntityNotFound);
            }
            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogGetSessionEntityError(ex, sessionId.ToString());
            return VKResult.Failure<PwpSessionEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IEnumerable<PwpSessionEntity>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var entities = await _repository.GetListAsync(e => true, cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success<IEnumerable<PwpSessionEntity>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogListSessionEntitiesError(ex);
            return VKResult.Failure<IEnumerable<PwpSessionEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> CreateAsync(PwpSessionEntity entity, CancellationToken cancellationToken = default)
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

    public async Task<VKResult> UpdateAsync(PwpSessionEntity entity, CancellationToken cancellationToken = default)
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

            existing.UserId = entity.UserId;
            existing.PersonaId = entity.PersonaId;
            existing.Mode = entity.Mode;
            existing.ParentSessionId = entity.ParentSessionId;
            existing.ForkSourceSessionId = entity.ForkSourceSessionId;
            existing.ForkPointRef = entity.ForkPointRef;
            existing.Status = entity.Status;
            existing.TurnCount = entity.TurnCount;
            existing.UpdatedAt = entity.UpdatedAt;
            existing.LastActivityAt = entity.LastActivityAt;
            existing.CustomModelId = entity.CustomModelId;
            existing.CustomApiKey = entity.CustomApiKey;
            existing.CustomServiceType = entity.CustomServiceType;
            existing.CustomEndpoint = entity.CustomEndpoint;
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
