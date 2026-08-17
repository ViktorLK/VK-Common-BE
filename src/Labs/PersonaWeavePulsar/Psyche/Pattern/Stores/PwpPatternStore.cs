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
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Stores;

/// <summary>
/// SQLite implementation of Psyche's <see cref="IVKPatternStore"/>.
/// Focuses purely on AI runtime pattern retrieval for the Psyche pipeline.
/// Uses IVKReadRepository for strict read-only isolation.
/// </summary>
internal sealed class PwpPatternStore : IVKPatternStore
{
    private readonly IVKReadRepository<PwpPatternEntity> _repository;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly ILogger<PwpPatternStore> _logger;

    public PwpPatternStore(
        IVKReadRepository<PwpPatternEntity> repository,
        IVKPsycheModelFactory modelFactory,
        ILogger<PwpPatternStore> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<IEnumerable<VKPatternEntry>>> GetPatternsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var entities = await _repository.GetListAsync(
                e => e.Segment.IsEnabled,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var domainEntries = entities.Select(MapToDomain).ToList();
            return VKResult.Success<IEnumerable<VKPatternEntry>>(domainEntries);
        }
        catch (Exception ex)
        {
            _logger.LogGetCurrentPatternsError(ex);
            return VKResult.Failure<IEnumerable<VKPatternEntry>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    private VKPatternEntry MapToDomain(PwpPatternEntity entity)
    {
        return _modelFactory.CreatePattern(
            entity.Id,
            entity.Segment.ToDomainSegment()
        );
    }
}
