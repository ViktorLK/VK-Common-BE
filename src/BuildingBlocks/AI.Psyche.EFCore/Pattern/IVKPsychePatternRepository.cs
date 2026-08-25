using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Industrial-grade repository interface for managing pattern entities.
/// Follows AP.01, CS.01, CS.03.
/// </summary>
public interface IVKPsychePatternRepository
{
    Task<VKResult<VKPsychePatternEntity>> GetByIdAsync(VKPatternId patternId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<VKPsychePatternEntity>>> GetListAsync(CancellationToken cancellationToken = default);
    Task<VKResult> CreateAsync(VKPsychePatternEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(VKPsychePatternEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKPatternId patternId, CancellationToken cancellationToken = default);
}
