using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Blocks.Persistence;
using VK.Labs.PersonaWeavePulsar.Persistence;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Diagnostics;
using VK.Labs.PersonaWeavePulsar.Psyche.Echo.Entities;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Echo.Stores;

/// <summary>
/// SQLite implementation of Psyche's <see cref="IVKEchoStore"/> using VK.Blocks.Persistence repositories.
/// Serves as the Echo / Short-term memory store for the Psyche Weaving Pipeline.
/// Follows CS.01 (Result<T>), CS.03 (ConfigureAwait(false)), and AP.01.
/// </summary>
public sealed class PwpEchoStore : IVKEchoStore
{
    private readonly IVKBaseRepository<PwpEchoEntity> _echoRepository;
    private readonly IVKUnitOfWork<PwpDbContext> _unitOfWork;
    private readonly IVKPsycheModelFactory _modelFactory;
    private readonly ILogger<PwpEchoStore> _logger;

    public PwpEchoStore(
        IVKBaseRepository<PwpEchoEntity> messageRepository,
        IVKUnitOfWork<PwpDbContext> unitOfWork,
        IVKPsycheModelFactory modelFactory,
        ILogger<PwpEchoStore> logger)
    {
        _echoRepository = VKGuard.NotNull(messageRepository);
        _unitOfWork = VKGuard.NotNull(unitOfWork);
        _modelFactory = VKGuard.NotNull(modelFactory);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotDefault(sessionId);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var entities = await _echoRepository.QueryAsync(
                q => q.Where(e => e.SessionId == sessionId).OrderBy(e => e.CreatedAt),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var traces = entities.Select(MapToDomain).ToList();
            return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(traces);
        }
        catch (Exception ex)
        {
            _logger.LogGetChatHistoryError(ex, sessionId.ToString());
            return VKResult.Failure<IReadOnlyCollection<VKEchoTrace>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> SaveHistoryAsync(
        VKEchoTrace trace,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(trace);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var entity = MapToEntity(trace);
            await _echoRepository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogSaveEchoTraceError(ex, trace.Id.ToString(), trace.SessionId.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    private VKEchoTrace MapToDomain(PwpEchoEntity entity)
    {
        return _modelFactory.CreateEcho(
            entity.Id,
            entity.SessionId,
            entity.Role,
            entity.Content ?? string.Empty,
            entity.TokenCount,
            entity.CreatedAt,
            entity.TenantId
        );
    }

    private static PwpEchoEntity MapToEntity(VKEchoTrace trace)
    {
        return new PwpEchoEntity
        {
            TenantId = trace.TenantId,
            SessionId = trace.SessionId,
            Id = trace.Id,
            Role = trace.Role,
            Content = trace.Content,
            TokenCount = trace.TokenCount,
            CreatedAt = trace.CreatedAt
        };
    }
}
