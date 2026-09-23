using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsychePersonaRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryPersonaRepository : VKInMemoryAggregateRepository<VKPersonaAnchor, VKPersonaId>, IVKPsychePersonaRepository
{
    public InMemoryPersonaRepository()
    {
    }

    protected override VKError GetNotFoundError(VKPersonaId id) => VKPersonaErrors.NotFound;

    protected override VKError GetAlreadyExistsError(VKPersonaId id) => VKPersonaErrors.AlreadyExists;
}
