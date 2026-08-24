using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Pattern.Internal;

/// <summary>
/// EFCore implementation of Psyche's <see cref="IVKPatternStore"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class PatternStore : IVKPatternStore
{
    private readonly IVKReadRepository<VKPsychePatternEntity> _repository;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly ILogger<PatternStore> _logger;

    public PatternStore(
        IVKReadRepository<VKPsychePatternEntity> repository,
        IVKPsycheModelFactory modelFactory,
        ILogger<PatternStore> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<IReadOnlyList<VKPatternEntry>>> GetPatternsAsync(
        IReadOnlyList<VKPatternId> patternIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(patternIds);

        if (patternIds.Count == 0)
        {
            return VKResult.Success<IReadOnlyList<VKPatternEntry>>([]);
        }

        try
        {
            var entities = await _repository.GetListAsync(
                e => patternIds.Contains(e.Id),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var domainList = entities.Select(MapToDomain).ToList();
            return VKResult.Success<IReadOnlyList<VKPatternEntry>>(domainList);
        }
        catch (Exception ex)
        {
            _logger.LogGetPatternsStoreError(ex, string.Join(",", patternIds));
            return VKResult.Failure<IReadOnlyList<VKPatternEntry>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    private VKPatternEntry MapToDomain(VKPsychePatternEntity entity)
    {
        var segment = _modelFactory.CreateSegment(
            entity.Content,
            entity.Name,
            entity.IsEnabled,
            entity.Role,
            entity.AbsoluteDepth,
            entity.RelativeDepth,
            entity.DepthPriority);

        return _modelFactory.CreatePattern(entity.Id, segment);
    }
}
