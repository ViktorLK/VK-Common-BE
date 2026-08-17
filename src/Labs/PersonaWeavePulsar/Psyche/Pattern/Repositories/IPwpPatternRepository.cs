using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Repositories;

/// <summary>
/// Industrial-grade repository interface for managing preset pattern entities in PWP.
/// </summary>
public interface IPwpPatternRepository
{
    Task<VKResult<PwpPatternEntity>> GetByIdAsync(VKPatternId patternId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<PwpPatternEntity>>> GetListAsync(CancellationToken cancellationToken = default);
    Task<VKResult> CreateAsync(PwpPatternEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(PwpPatternEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKPatternId patternId, CancellationToken cancellationToken = default);
}
