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

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona.Stores;

/// <summary>
/// SQLite implementation of Psyche's <see cref="IVKPersonaStore"/>.
/// Focuses purely on AI runtime persona retrieval for the Psyche pipeline.
/// Uses IVKReadRepository for strict read-only isolation and IVKPsycheModelFactory for domain mapping.
/// </summary>
public sealed class PwpPersonaStore : IVKPersonaStore
{
    private readonly IVKReadRepository<PwpPersonaEntity> _repository;
    private readonly IVKJsonSerializer _serializer;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly ILogger<PwpPersonaStore> _logger;

    public PwpPersonaStore(
        IVKReadRepository<PwpPersonaEntity> repository,
        IVKJsonSerializer serializer,
        IVKPsycheModelFactory modelFactory,
        ILogger<PwpPersonaStore> logger)
    {
        _repository = VKGuard.NotNull(repository);
        _serializer = VKGuard.NotNull(serializer);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKPersonaAnchor>> GetPersonaAsync(
        VKPersonaId personaId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(personaId);

        try
        {
            var entity = await _repository.GetFirstOrDefaultAsync(
                e => e.Id == personaId,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (entity is null)
            {
                return VKResult.Failure<VKPersonaAnchor>(VKPersonaErrors.NotFound);
            }

            return VKResult.Success(MapToDomain(entity));
        }
        catch (Exception ex)
        {
            _logger.LogGetPersonaError(ex, personaId.ToString());
            return VKResult.Failure<VKPersonaAnchor>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    private VKPersonaAnchor MapToDomain(PwpPersonaEntity entity)
    {
        var traits = _serializer.DeserializeOrDefault<Dictionary<string, string>>(entity.Traits, []);

        return _modelFactory.CreatePersona(
            entity.Id,
            entity.Name,
            entity.Description ?? string.Empty,
            traits,
            entity.DirectiveId?.ToString(),
            tenantId: entity.TenantId);
    }
}
