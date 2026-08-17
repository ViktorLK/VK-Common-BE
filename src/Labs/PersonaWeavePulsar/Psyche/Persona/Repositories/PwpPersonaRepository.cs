using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Persistence;
using VK.Labs.PersonaWeavePulsar.Psyche.Persona.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Persona.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona.Repositories;

/// <summary>
/// Industrial-grade repository implementation for PWP Persona entity CRUD operations (<see cref="IPwpPersonaRepository"/>).
/// </summary>
public sealed class PwpPersonaRepository : IPwpPersonaRepository
{
    private readonly IVKBaseRepository<PwpPersonaEntity> _repository;
    private readonly IVKUnitOfWork<PwpDbContext> _unitOfWork;
    private readonly ILogger<PwpPersonaRepository> _logger;

    public PwpPersonaRepository(
        IVKBaseRepository<PwpPersonaEntity> repository,
        IVKUnitOfWork<PwpDbContext> unitOfWork,
        ILogger<PwpPersonaRepository> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<PwpPersonaEntity>> GetByIdAsync(VKPersonaId personaId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(personaId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == personaId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Failure<PwpPersonaEntity>(VKPersistenceErrors.Repository.EntityNotFound);
            }
            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogGetPersonaEntityError(ex, personaId.ToString());
            return VKResult.Failure<PwpPersonaEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IEnumerable<PwpPersonaEntity>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var entities = await _repository.GetListAsync(e => true, cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success<IEnumerable<PwpPersonaEntity>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogListPersonaEntitiesError(ex);
            return VKResult.Failure<IEnumerable<PwpPersonaEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<PwpPersonaEntity>> CreateAsync(PwpPersonaEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entity);

        try
        {
            await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogCreatePersonaEntityError(ex, entity.Id.ToString());
            return VKResult.Failure<PwpPersonaEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(PwpPersonaEntity entity, CancellationToken cancellationToken = default)
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

            existing.Name = entity.Name;
            existing.Description = entity.Description;
            existing.Traits = entity.Traits;
            existing.DirectiveId = entity.DirectiveId;

            await _repository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogUpdatePersonaEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> DeleteAsync(VKPersonaId personaId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(personaId);

        try
        {
            var existing = await _repository.GetFirstOrDefaultAsync(e => e.Id == personaId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                await _repository.DeleteAsync(existing, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogDeletePersonaEntityError(ex, personaId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
