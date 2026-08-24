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
internal sealed class SessionStore : IVKSessionStore
{
    private readonly IVKBaseRepository<VKPsycheSessionEntity> _sessionRepository;
    private readonly IVKUnitOfWork _unitOfWork;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly ILogger<SessionStore> _logger;

    public SessionStore(
        IVKBaseRepository<VKPsycheSessionEntity> sessionRepository,
        IVKUnitOfWork unitOfWork,
        IVKPsycheModelFactory modelFactory,
        IVKJsonSerializer jsonSerializer,
        ILogger<SessionStore> logger)
    {
        _sessionRepository = VKGuard.NotNull(sessionRepository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _logger = VKGuard.NotNull(logger);
    }

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

            if (entity is null)
            {
                return VKResult.Success<VKSessionThread?>(null);
            }

            return VKResult.Success<VKSessionThread?>(MapToDomain(entity));
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

            existing.Mode = session.Mode;
            existing.ParentSessionId = session.ParentSessionId;
            existing.ForkSourceSessionId = session.ForkSourceSessionId;
            existing.ForkPointRef = session.ForkPointRef;
            existing.Status = session.Status;
            existing.TurnCount = session.TurnCount;
            existing.UpdatedAt = session.UpdatedAt;
            existing.LastActivityAt = session.LastActivityAt;
            existing.KnowledgeStateJson = _jsonSerializer.Serialize(session.KnowledgeState);

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

    private VKSessionThread MapToDomain(VKPsycheSessionEntity entity)
    {
        return _modelFactory.CreateSession(
            entity.Id,
            entity.Mode,
            entity.ParentSessionId,
            entity.ForkSourceSessionId,
            entity.ForkPointRef,
            entity.Status,
            entity.TurnCount,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.LastActivityAt,
            entity.ToKnowledgeState(_jsonSerializer)
        );
    }
}
