using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Echo.Repositories;

/// <summary>
/// Industrial-grade repository interface for managing Echo trace entities.
/// </summary>
public interface IPwpEchoRepository
{
    Task<VKResult<IReadOnlyCollection<PwpEchoEntity>>> GetHistoryAsync(VKSessionId sessionId, int limit = 50, CancellationToken cancellationToken = default);
    Task<VKResult> CreateAsync(PwpEchoEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> UpdateAsync(PwpEchoEntity entity, CancellationToken cancellationToken = default);
    Task<VKResult> DeleteAsync(VKSessionId sessionId, VKEchoId traceId, CancellationToken cancellationToken = default);
    Task<VKResult> ClearHistoryAsync(VKSessionId sessionId, CancellationToken cancellationToken = default);
}
