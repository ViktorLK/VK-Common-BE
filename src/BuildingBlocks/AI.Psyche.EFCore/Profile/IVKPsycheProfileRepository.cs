using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Industrial-grade repository interface for managing profile presence entities.
/// Follows AP.01, CS.01, CS.03.
/// </summary>
public interface IVKPsycheProfileRepository
{
    Task<VKResult<VKPsycheProfileEntity>> GetByIdAsync(VKProfileId profileId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<VKPsycheProfileEntity>>> GetListAsync(CancellationToken cancellationToken = default);
    Task<VKResult> CreateAsync(VKPsycheProfileEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(VKPsycheProfileEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKProfileId profileId, CancellationToken cancellationToken = default);
}
