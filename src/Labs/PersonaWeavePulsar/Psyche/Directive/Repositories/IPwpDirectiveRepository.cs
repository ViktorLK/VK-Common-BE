using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Directive.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Directive.Repositories;

/// <summary>
/// Industrial-grade repository interface for managing tenant directive entities.
/// </summary>
public interface IPwpDirectiveRepository
{
    Task<VKResult<PwpDirectiveEntity>> GetByIdAsync(VKDirectiveId directiveId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<PwpDirectiveEntity>>> GetListAsync(CancellationToken cancellationToken = default);
    Task<VKResult> CreateAsync(PwpDirectiveEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(PwpDirectiveEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKDirectiveId directiveId, CancellationToken cancellationToken = default);
}
