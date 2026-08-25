using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Persona.Internal;

/// <summary>
/// EFCore implementation of Psyche's <see cref="IVKPersonaStore"/>.
/// Focuses purely on AI runtime persona retrieval for the Psyche pipeline.
/// </summary>
internal sealed class PersonaStore : IVKPersonaStore
{
    private readonly IVKReadRepository<VKPsychePersonaEntity> _repository;
    private readonly IVKJsonSerializer _serializer;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly ILogger<PersonaStore> _logger;

    public PersonaStore(
        IVKReadRepository<VKPsychePersonaEntity> repository,
        IVKJsonSerializer serializer,
        IVKPsycheModelFactory modelFactory,
        ILogger<PersonaStore> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _serializer = VKGuard.NotNull(serializer);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<IReadOnlyList<VKPersonaAnchor>>> GetPersonasAsync(
        IReadOnlyList<VKPersonaId> personaIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(personaIds);

        if (personaIds.Count == 0)
        {
            return VKResult.Success<IReadOnlyList<VKPersonaAnchor>>([]);
        }

        try
        {
            var entities = await _repository.GetListAsync(
                e => personaIds.Contains(e.Id),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var domainList = entities.Select(MapToDomain).ToList();
            return VKResult.Success<IReadOnlyList<VKPersonaAnchor>>(domainList);
        }
        catch (Exception ex)
        {
            _logger.LogGetPersonaError(ex, string.Join(",", personaIds));
            return VKResult.Failure<IReadOnlyList<VKPersonaAnchor>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    private VKPersonaAnchor MapToDomain(VKPsychePersonaEntity entity)
    {
        var traits = _serializer.DeserializeOrDefault<Dictionary<string, string>>(entity.Traits, []);

        return _modelFactory.CreatePersona(
            entity.Id,
            entity.Name,
            entity.Description ?? string.Empty,
            traits,
            entity.DirectiveId?.ToString());
    }
}
