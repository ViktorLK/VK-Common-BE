using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Persona.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona.Repositories;

/// <summary>
/// Industrial-grade repository interface for managing persona entities.
/// </summary>
public interface IPwpPersonaRepository
{
    Task<VKResult<PwpPersonaEntity>> GetByIdAsync(VKPersonaId personaId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<PwpPersonaEntity>>> GetListAsync(CancellationToken cancellationToken = default);
    Task<VKResult<PwpPersonaEntity>> CreateAsync(PwpPersonaEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(PwpPersonaEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKPersonaId personaId, CancellationToken cancellationToken = default);
}
