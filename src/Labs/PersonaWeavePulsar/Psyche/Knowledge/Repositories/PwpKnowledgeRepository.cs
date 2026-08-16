using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Corpus;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Features.KnowledgeBook.Entities;
using VK.Labs.PersonaWeavePulsar.Persistence;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Repositories;

/// <summary>
/// Industrial-grade repository implementation for PWP Lorebook and Knowledge entity CRUD operations (<see cref="IPwpKnowledgeRepository"/>).
/// </summary>
public sealed class PwpKnowledgeRepository : IPwpKnowledgeRepository
{
    private readonly IVKBaseRepository<PwpKnowledgeEntity> _entryRepository;
    private readonly IVKBaseRepository<PwpKnowledgeKeyEntity> _keyRepository;
    private readonly IVKBulkRepository<PwpKnowledgeKeyEntity> _keyBulkRepository;
    private readonly IVKUnitOfWork<PwpDbContext> _unitOfWork;
    private readonly ILogger<PwpKnowledgeRepository> _logger;

    public PwpKnowledgeRepository(
        IVKBaseRepository<PwpKnowledgeEntity> entryRepository,
        IVKBaseRepository<PwpKnowledgeKeyEntity> keyRepository,
        IVKBulkRepository<PwpKnowledgeKeyEntity> keyBulkRepository,
        IVKUnitOfWork<PwpDbContext> unitOfWork,
        ILogger<PwpKnowledgeRepository> logger)
    {
        _entryRepository = VKGuard.NotNull(entryRepository);
        _keyRepository = VKGuard.NotNull(keyRepository);
        _keyBulkRepository = VKGuard.NotNull(keyBulkRepository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<PwpKnowledgeEntity>> GetByIdAsync(VKKnowledgeId knowledgeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(knowledgeId);

        try
        {
            var entity = await _entryRepository.GetFirstOrDefaultAsync(
                e => e.Id == knowledgeId,
                q => q.Include(e => e.Keys),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (entity is null)
            {
                return VKResult.Failure<PwpKnowledgeEntity>(VKPersistenceErrors.Repository.EntityNotFound);
            }
            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogGetKnowledgeEntityError(ex, knowledgeId.ToString());
            return VKResult.Failure<PwpKnowledgeEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IEnumerable<PwpKnowledgeEntity>>> GetListAsync(
        PwpKnowledgeBookId? knowledgeBookId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var entries = await _entryRepository.GetListAsync(
                knowledgeBookId is null
                    ? _ => true
                    : e => e.KnowledgeBookId == knowledgeBookId.Value,
                include: q => q.Include(e => e.Keys),
                options: null,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return VKResult.Success<IEnumerable<PwpKnowledgeEntity>>(entries);
        }
        catch (Exception ex)
        {
            _logger.LogListKnowledgeEntitiesError(ex);
            return VKResult.Failure<IEnumerable<PwpKnowledgeEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> CreateAsync(
        PwpKnowledgeEntity entity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entity);

        try
        {
            await _entryRepository.AddAsync(entity, cancellationToken).ConfigureAwait(false);

            if (entity.Keys.Count > 0)
            {
                await _keyRepository.AddRangeAsync(entity.Keys.ToList(), cancellationToken).ConfigureAwait(false);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogCreateKnowledgeEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(
        PwpKnowledgeEntity entity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entity);

        try
        {
            var existing = await _entryRepository.GetFirstOrDefaultAsync(
                e => e.Id == entity.Id,
                include: q => q.Include(e => e.Keys),
                options: null,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (existing is null)
            {
                return VKResult.Failure(VKPersistenceErrors.Repository.EntityNotFound);
            }

            existing.KnowledgeBookId = entity.KnowledgeBookId;
            existing.TriggerType = entity.TriggerType;
            existing.FilterLogic = entity.FilterLogic;
            existing.Segment = entity.Segment;
            existing.StickyTurns = entity.StickyTurns;
            existing.CooldownTurns = entity.CooldownTurns;
            existing.DelayTurns = entity.DelayTurns;
            existing.ExclusiveGroup = entity.ExclusiveGroup;
            existing.ExclusiveWeight = entity.ExclusiveWeight;
            existing.Tag = entity.Tag;
            existing.StateConditions = entity.StateConditions;
            existing.Probability = entity.Probability;
            existing.MaxCount = entity.MaxCount;
            existing.MaxCountPerTurn = entity.MaxCountPerTurn;
            existing.StartTurn = entity.StartTurn;
            existing.EndTurn = entity.EndTurn;
            existing.ExclusionTag = entity.ExclusionTag;
            existing.DependencyId = entity.DependencyId;
            existing.ConflictGroupId = entity.ConflictGroupId;
            existing.MinAffection = entity.MinAffection;
            existing.MaxAnger = entity.MaxAnger;
            existing.RevealSecretKey = entity.RevealSecretKey;
            existing.TargetPersonaId = entity.TargetPersonaId;
            existing.ExpiresAt = entity.ExpiresAt;
            existing.UserSegment = entity.UserSegment;

            await _entryRepository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            await _keyBulkRepository.ExecuteDeleteAsync(k => k.KnowledgeEntryId == entity.Id, cancellationToken).ConfigureAwait(false);

            if (entity.Keys.Count > 0)
            {
                await _keyRepository.AddRangeAsync(entity.Keys.ToList(), cancellationToken).ConfigureAwait(false);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogUpdateKnowledgeEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> DeleteAsync(
        VKKnowledgeId entryId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(entryId);

        try
        {
            var entity = await _entryRepository.GetFirstOrDefaultAsync(e => e.Id == entryId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Success();
            }

            await _entryRepository.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogDeleteKnowledgeEntityError(ex, entryId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
