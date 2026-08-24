using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Industrial-grade repository interface for managing session thread entities.
/// Follows AP.01, CS.01, CS.03.
/// </summary>
public interface IVKPsycheSessionRepository
{
    Task<VKResult<VKPsycheSessionEntity>> GetByIdAsync(VKSessionId sessionId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<VKPsycheSessionEntity>>> GetListAsync(CancellationToken cancellationToken = default);
    Task<VKResult> CreateAsync(VKPsycheSessionEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(VKPsycheSessionEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKSessionId sessionId, CancellationToken cancellationToken = default);
}
