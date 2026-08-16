using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Session.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Session.Repositories;

/// <summary>
/// Industrial-grade repository interface for managing chat session entities.
/// </summary>
public interface IPwpSessionRepository
{
    Task<VKResult<PwpSessionEntity>> GetByIdAsync(VKSessionId sessionId, CancellationToken cancellationToken = default);
    Task<VKResult<IEnumerable<PwpSessionEntity>>> GetListAsync(CancellationToken cancellationToken = default);
    Task<VKResult> CreateAsync(PwpSessionEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(PwpSessionEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKSessionId sessionId, CancellationToken cancellationToken = default);
}
