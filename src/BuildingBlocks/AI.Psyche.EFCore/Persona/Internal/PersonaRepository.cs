using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Persona.Internal;

/// <summary>
/// Industrial-grade repository implementation for pure Psyche Persona persistence.
/// </summary>
internal sealed class PersonaRepository : IVKPsychePersonaRepository
{
    private readonly IVKBaseRepository<VKPsychePersonaEntity> _repository;
    private readonly IVKUnitOfWork _unitOfWork;
    private readonly ILogger<PersonaRepository> _logger;

    public PersonaRepository(
        IVKBaseRepository<VKPsychePersonaEntity> repository,
        IVKUnitOfWork unitOfWork,
        ILogger<PersonaRepository> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKPsychePersonaEntity>> GetByIdAsync(VKPersonaId personaId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(personaId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == personaId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Failure<VKPsychePersonaEntity>(VKPersistenceErrors.Repository.EntityNotFound);
            }

            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogGetPersonaEntityError(ex, personaId.ToString());
            return VKResult.Failure<VKPsychePersonaEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IEnumerable<VKPsychePersonaEntity>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var list = await _repository.GetListAsync(e => true, cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success<IEnumerable<VKPsychePersonaEntity>>(list);
        }
        catch (Exception ex)
        {
            _logger.LogListPersonaEntitiesError(ex);
            return VKResult.Failure<IEnumerable<VKPsychePersonaEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> CreateAsync(VKPsychePersonaEntity entity, CancellationToken cancellationToken = default)
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
            _logger.LogCreatePersonaEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(VKPsychePersonaEntity entity, CancellationToken cancellationToken = default)
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
            existing.Personality = entity.Personality;
            existing.Scenario = entity.Scenario;
            existing.FirstMessage = entity.FirstMessage;
            existing.DialogueExamples = entity.DialogueExamples;
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
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == personaId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is not null)
            {
                await _repository.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
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
