using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Session.Internal;

/// <summary>
/// EFCore implementation of Psyche's <see cref="IVKSessionStore"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class SessionStore(
    IVKEntityRepository<VKPsycheSessionEntity> sessionRepository,
    IVKUnitOfWork unitOfWork,
    ILogger<SessionStore> logger) : IVKSessionStore
{
    private readonly IVKEntityRepository<VKPsycheSessionEntity> _sessionRepository = VKGuard.NotNull(sessionRepository);
    private readonly IVKUnitOfWork _unitOfWork = VKGuard.NotNull(unitOfWork);
    private readonly ILogger<SessionStore> _logger = VKGuard.NotNull(logger);

    public async Task<VKResult<VKSessionThread?>> GetSessionAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (sessionId.IsEmpty)
        {
            return VKResult.Success<VKSessionThread?>(null);
        }

        try
        {
            var entity = await _sessionRepository.GetFirstOrDefaultAsync(
                e => e.Id == sessionId,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return VKResult.Success(entity?.ToDomain());
        }
        catch (Exception ex)
        {
            _logger.LogGetSessionError(ex, sessionId.ToString());
            return VKResult.Failure<VKSessionThread?>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> UpdateSessionAsync(
        VKSessionThread session,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(session);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var existing = await _sessionRepository.GetTrackedFirstOrDefaultAsync(
                s => s.Id == session.Id,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (existing is null)
            {
                return VKResult.Failure(VKPersistenceErrors.Repository.EntityNotFound);
            }

            session.MapOnto(existing);

            await _sessionRepository.UpdateAsync(existing, cancellationToken: cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogSaveSessionError(ex, session.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
