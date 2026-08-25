using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Profile.Internal;

/// <summary>
/// Industrial-grade repository implementation for pure Psyche Profile presence persistence.
/// </summary>
internal sealed class ProfileRepository : IVKPsycheProfileRepository
{
    private readonly IVKBaseRepository<VKPsycheProfileEntity> _repository;
    private readonly IVKUnitOfWork _unitOfWork;
    private readonly ILogger<ProfileRepository> _logger;

    public ProfileRepository(
        IVKBaseRepository<VKPsycheProfileEntity> repository,
        IVKUnitOfWork unitOfWork,
        ILogger<ProfileRepository> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKPsycheProfileEntity>> GetByIdAsync(VKProfileId profileId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(profileId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == profileId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Failure<VKPsycheProfileEntity>(VKPersistenceErrors.Repository.EntityNotFound);
            }

            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogGetProfileEntityError(ex, profileId.ToString());
            return VKResult.Failure<VKPsycheProfileEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IEnumerable<VKPsycheProfileEntity>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var list = await _repository.GetListAsync(e => true, cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success<IEnumerable<VKPsycheProfileEntity>>(list);
        }
        catch (Exception ex)
        {
            _logger.LogListProfileEntitiesError(ex);
            return VKResult.Failure<IEnumerable<VKPsycheProfileEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> CreateAsync(VKPsycheProfileEntity entity, CancellationToken cancellationToken = default)
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
            _logger.LogCreateProfileEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateAsync(VKPsycheProfileEntity entity, CancellationToken cancellationToken = default)
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

            existing.DisplayName = entity.DisplayName;
            existing.PreferredLanguage = entity.PreferredLanguage;
            existing.TimeZone = entity.TimeZone;
            existing.PreferencesJson = entity.PreferencesJson;

            await _repository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogUpdateProfileEntityError(ex, entity.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> DeleteAsync(VKProfileId profileId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(profileId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == profileId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is not null)
            {
                await _repository.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogDeleteProfileEntityError(ex, profileId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
