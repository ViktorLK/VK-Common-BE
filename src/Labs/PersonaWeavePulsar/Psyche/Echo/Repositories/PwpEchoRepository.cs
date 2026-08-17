using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Persistence;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Echo.Repositories;

/// <summary>
/// Industrial-grade repository implementation for PWP Echo entity CRUD operations (<see cref="IPwpEchoRepository"/>).
/// </summary>
public sealed class PwpEchoRepository : IPwpEchoRepository
{
    private readonly IVKBaseRepository<PwpEchoEntity> _repository;
    private readonly IVKBulkRepository<PwpEchoEntity> _bulkRepository;
    private readonly IVKUnitOfWork<PwpDbContext> _unitOfWork;
    private readonly ILogger<PwpEchoRepository> _logger;

    public PwpEchoRepository(
        IVKBaseRepository<PwpEchoEntity> repository,
        IVKBulkRepository<PwpEchoEntity> bulkRepository,
        IVKUnitOfWork<PwpDbContext> unitOfWork,
        ILogger<PwpEchoRepository> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _bulkRepository = VKGuard.NotNull(bulkRepository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<IReadOnlyCollection<PwpEchoEntity>>> GetHistoryAsync(VKSessionId sessionId, int limit = 50, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(sessionId);

        try
        {
            var entities = await _repository.QueryAsync(
                q => q.Where(e => e.SessionId == sessionId).OrderByDescending(e => e.CreatedAt).Take(limit),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var list = entities.OrderBy(e => e.CreatedAt).ToList();
            return VKResult.Success<IReadOnlyCollection<PwpEchoEntity>>(list);
        }
        catch (Exception ex)
        {
            _logger.LogGetChatHistoryError(ex, sessionId.ToString());
            return VKResult.Failure<IReadOnlyCollection<PwpEchoEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> CreateAsync(PwpEchoEntity entity, CancellationToken cancellationToken = default)
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
            _logger.LogCreateEchoEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(PwpEchoEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entity);
        VKGuard.NotDefault(entity.SessionId);

        try
        {
            var existing = await _repository.GetFirstOrDefaultAsync(
                e => e.Id == entity.Id && e.SessionId == entity.SessionId,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                return VKResult.Failure(VKPersistenceErrors.Repository.EntityNotFound);
            }

            existing.Content = entity.Content;
            existing.Role = entity.Role;
            existing.TokenCount = entity.TokenCount;

            await _repository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogUpdateEchoEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> DeleteAsync(VKSessionId sessionId, VKEchoId traceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(sessionId);
        VKGuard.NotDefault(traceId);

        try
        {
            var existing = await _repository.GetFirstOrDefaultAsync(
                e => e.Id == traceId && e.SessionId == sessionId,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                await _repository.DeleteAsync(existing, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogDeleteEchoEntityError(ex, traceId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> ClearHistoryAsync(VKSessionId sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(sessionId);

        try
        {
            await _bulkRepository.ExecuteDeleteAsync(e => e.SessionId == sessionId, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogClearHistoryError(ex, sessionId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
