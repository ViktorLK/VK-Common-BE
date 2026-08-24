using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Industrial-grade repository interface for managing persona entities.
/// Follows AP.01, CS.01, CS.03.
/// </summary>
public interface IVKPsychePersonaRepository
{
    Task<VKResult<VKPsychePersonaEntity>> GetByIdAsync(VKPersonaId personaId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<VKPsychePersonaEntity>>> GetListAsync(CancellationToken cancellationToken = default);
    Task<VKResult> CreateAsync(VKPsychePersonaEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(VKPsychePersonaEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKPersonaId personaId, CancellationToken cancellationToken = default);
}
