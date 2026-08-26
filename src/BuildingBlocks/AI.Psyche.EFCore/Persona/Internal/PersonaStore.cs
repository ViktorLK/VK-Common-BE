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
internal sealed class PersonaStore(
    IVKEntityReadRepository<VKPsychePersonaEntity> repository,
    ILogger<PersonaStore> logger) : IVKPersonaStore
{
    private readonly IVKEntityReadRepository<VKPsychePersonaEntity> _repository = VKGuard.NotNull(repository);
    private readonly ILogger<PersonaStore> _logger = VKGuard.NotNull(logger);

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

            var domainList = entities.Select(e => e.ToDomain()).ToList();
            return VKResult.Success<IReadOnlyList<VKPersonaAnchor>>(domainList);
        }
        catch (Exception ex)
        {
            _logger.LogGetPersonaError(ex, string.Join(",", personaIds));
            return VKResult.Failure<IReadOnlyList<VKPersonaAnchor>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
