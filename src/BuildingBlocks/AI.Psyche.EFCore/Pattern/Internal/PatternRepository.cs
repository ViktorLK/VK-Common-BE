using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Pattern.Internal;

/// <summary>
/// Industrial-grade repository implementation for pure Psyche Pattern persistence.
/// </summary>
internal sealed class PatternRepository : IVKPsychePatternRepository
{
    private readonly IVKBaseRepository<VKPsychePatternEntity> _repository;
    private readonly IVKUnitOfWork _unitOfWork;
    private readonly ILogger<PatternRepository> _logger;

    public PatternRepository(
        IVKBaseRepository<VKPsychePatternEntity> repository,
        IVKUnitOfWork unitOfWork,
        ILogger<PatternRepository> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKPsychePatternEntity>> GetByIdAsync(VKPatternId patternId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(patternId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == patternId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return VKResult.Failure<VKPsychePatternEntity>(VKPersistenceErrors.Repository.EntityNotFound);
            }

            return VKResult.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogGetPatternEntityError(ex, patternId.ToString());
            return VKResult.Failure<VKPsychePatternEntity>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IEnumerable<VKPsychePatternEntity>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var list = await _repository.GetListAsync(e => true, cancellationToken: cancellationToken).ConfigureAwait(false);
            return VKResult.Success<IEnumerable<VKPsychePatternEntity>>(list);
        }
        catch (Exception ex)
        {
            _logger.LogListPatternEntitiesError(ex);
            return VKResult.Failure<IEnumerable<VKPsychePatternEntity>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> CreateAsync(VKPsychePatternEntity entity, CancellationToken cancellationToken = default)
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

    public async Task<VKResult> UpdateAsync(VKPsychePatternEntity entity, CancellationToken cancellationToken = default)
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

            existing.Content = entity.Content;
            existing.Name = entity.Name;
            existing.IsEnabled = entity.IsEnabled;
            existing.Role = entity.Role;
            existing.AbsoluteDepth = entity.AbsoluteDepth;
            existing.RelativeDepth = entity.RelativeDepth;
            existing.DepthPriority = entity.DepthPriority;

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
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Id == patternId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (entity is not null)
            {
                await _repository.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
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
