using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Directive.Stores;

/// <summary>
/// SQLite implementation of Psyche's <see cref="IVKDirectiveStore"/>.
/// Focuses purely on AI runtime directive charter retrieval for the Psyche pipeline.
/// Uses IVKReadRepository for strict read-only isolation and IVKPsycheModelFactory for domain mapping.
/// </summary>
public sealed class PwpDirectiveStore : IVKDirectiveStore
{
    private readonly IVKReadRepository<PwpDirectiveEntity> _repository;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly ILogger<PwpDirectiveStore> _logger;

    public PwpDirectiveStore(
        IVKReadRepository<PwpDirectiveEntity> repository,
        IVKPsycheModelFactory modelFactory,
        ILogger<PwpDirectiveStore> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKDirectiveCharter>> GetDirectiveAsync(
        VKDirectiveId directiveId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(directiveId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(
                e => e.Id == directiveId,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (entity is null)
            {
                return VKResult.Failure<VKDirectiveCharter>(VKDirectiveErrors.NotFound);
            }

            return VKResult.Success(MapToDomain(entity));
        }
        catch (Exception ex)
        {
            _logger.LogGetDirectiveError(ex, directiveId.ToString());
            return VKResult.Failure<VKDirectiveCharter>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    private VKDirectiveCharter MapToDomain(PwpDirectiveEntity entity)
    {
        return _modelFactory.CreateDirective(
            entity.Id,
            entity.Overview,
            entity.BehaviorRules,
            entity.SafetyRules,
            entity.OutputConstraints
        );
    }
}
