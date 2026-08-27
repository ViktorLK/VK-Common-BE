using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Echo.Internal;

/// <summary>
/// EFCore implementation of Psyche's <see cref="IVKEchoStore"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class EchoStore(
    IVKEntityRepository<VKPsycheEchoEntity> repository,
    IVKUnitOfWork unitOfWork,
    ILogger<EchoStore> logger) : IVKEchoStore
{
    private readonly IVKEntityRepository<VKPsycheEchoEntity> _repository = VKGuard.NotNull(repository);
    private readonly IVKUnitOfWork _unitOfWork = VKGuard.NotNull(unitOfWork);
    private readonly ILogger<EchoStore> _logger = VKGuard.NotNull(logger);

    public async Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(sessionId);

        try
        {
            var entities = await _repository.GetListAsync(
                e => e.SessionId == sessionId,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var domainList = entities.OrderBy(e => e.CreatedAt).Select(e => e.ToDomain()).ToList();
            return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(domainList);
        }
        catch (Exception ex)
        {
            _logger.LogGetHistoryStoreError(ex, sessionId.ToString());
            return VKResult.Failure<IReadOnlyCollection<VKEchoTrace>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult> SaveHistoryAsync(
        VKEchoTrace trace,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(trace);

        try
        {
            var entity = trace.ToEntity();

            await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogSaveHistoryStoreError(ex, trace.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
