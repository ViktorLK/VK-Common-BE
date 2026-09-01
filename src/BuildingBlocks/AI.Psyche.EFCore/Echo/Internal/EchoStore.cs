using System;
using System.Collections.Generic;
using System.Diagnostics;
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
[VKTrace("psyche.efcore.echo_store")]
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

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var entities = await _repository.GetListAsync(
                e => e.SessionId == sessionId,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var domainList = entities.OrderBy(e => e.CreatedAt).Select(e => e.ToDomain()).ToList();
            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "GetHistory", true);
            return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(domainList);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "GetHistory", false);
            EchoDiagnostics.RecordEchoError("GetHistory");
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

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var entity = trace.ToEntity();

            await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "SaveHistory", true);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "SaveHistory", false);
            EchoDiagnostics.RecordEchoError("SaveHistory");
            _logger.LogSaveHistoryStoreError(ex, trace.Id.ToString());
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
