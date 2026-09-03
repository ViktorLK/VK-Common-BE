using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.Persistence;

namespace VK.Blocks.AI.Psyche.EFCore.Echo.Internal;

/// <summary>
/// EFCore implementation of Psyche's <see cref="IVKEchoStore"/>.
/// Supports high-performance 2-phase retrieval with EF Core Compiled Queries (zero LINQ-compilation allocation).
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
[VKTrace("psyche.efcore.echo_store")]
internal sealed class EchoStore(
    DbContext dbContext,
    IVKEntityRepository<VKPsycheEchoEntity> repository,
    IVKUnitOfWork unitOfWork,
    ILogger<EchoStore> logger) : IVKEchoStore
{
    private readonly DbContext _dbContext = VKGuard.NotNull(dbContext);
    private readonly IVKEntityRepository<VKPsycheEchoEntity> _repository = VKGuard.NotNull(repository);
    private readonly IVKUnitOfWork _unitOfWork = VKGuard.NotNull(unitOfWork);
    private readonly ILogger<EchoStore> _logger = VKGuard.NotNull(logger);

    // 1. Static Compiled Query for Phase 1 Metadata Projection (Zero-allocation LINQ, precompiled SQL)
    private static readonly Func<DbContext, VKSessionId, IAsyncEnumerable<VKEchoMetadata>> s_getMetadataCompiled =
        EF.CompileAsyncQuery((DbContext ctx, VKSessionId sessionId) =>
            ctx.Set<VKPsycheEchoEntity>()
               .AsNoTracking()
               .Where(e => e.SessionId == sessionId)
               .OrderBy(e => e.CreatedAt)
               .Select(e => new VKEchoMetadata
               {
                   Id = e.Id,
                   SessionId = e.SessionId,
                   Role = e.Role,
                   TokenCount = e.TokenCount,
                   CreatedAt = e.CreatedAt
               }));

    // 2. Static Compiled Query for Full History retrieval
    private static readonly Func<DbContext, VKSessionId, IAsyncEnumerable<VKPsycheEchoEntity>> s_getHistoryCompiled =
        EF.CompileAsyncQuery<DbContext, VKSessionId, VKPsycheEchoEntity>((DbContext ctx, VKSessionId sessionId) =>
            ctx.Set<VKPsycheEchoEntity>()
               .AsNoTracking()
               .Where(e => e.SessionId == sessionId)
               .OrderBy(e => e.CreatedAt));

    public async Task<VKResult<IReadOnlyCollection<VKEchoMetadata>>> GetMetadataAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(sessionId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var metaList = new List<VKEchoMetadata>();
            await foreach (var meta in s_getMetadataCompiled(_dbContext, sessionId).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                metaList.Add(meta);
            }

            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "GetMetadata", true);
            return VKResult.Success<IReadOnlyCollection<VKEchoMetadata>>(metaList);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "GetMetadata", false);
            EchoDiagnostics.RecordEchoError("GetMetadata");
            _logger.LogGetHistoryStoreError(ex, sessionId.ToString());
            return VKResult.Failure<IReadOnlyCollection<VKEchoMetadata>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetTracesByIdsAsync(
        IReadOnlyCollection<VKEchoId> ids,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(ids);

        if (ids.Count == 0)
        {
            return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>([]);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var entities = await _repository.GetListAsync(
                e => ids.Contains(e.Id),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var domainList = entities.OrderBy(e => e.CreatedAt).Select(e => e.ToDomain()).ToList();
            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "GetTracesByIds", true);
            return VKResult.Success<IReadOnlyCollection<VKEchoTrace>>(domainList);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "GetTracesByIds", false);
            EchoDiagnostics.RecordEchoError("GetTracesByIds");
            _logger.LogGetHistoryStoreError(ex, string.Join(",", ids.Select(i => i.ToString())));
            return VKResult.Failure<IReadOnlyCollection<VKEchoTrace>>(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }

    public async Task<VKResult<IReadOnlyCollection<VKEchoTrace>>> GetHistoryAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotDefault(sessionId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var domainList = new List<VKEchoTrace>();
            await foreach (var entity in s_getHistoryCompiled(_dbContext, sessionId).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                domainList.Add(entity.ToDomain());
            }

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

        return await SaveHistoryBatchAsync([trace], cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> SaveHistoryBatchAsync(
        IReadOnlyCollection<VKEchoTrace> traces,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(traces);

        if (traces.Count == 0)
        {
            return VKResult.Success();
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var entities = traces.Select(t => t.ToEntity()).ToList();

            await _repository.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "SaveHistoryBatch", true);
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            EchoDiagnostics.RecordEchoOperation(stopwatch.Elapsed.TotalMilliseconds, "SaveHistoryBatch", false);
            EchoDiagnostics.RecordEchoError("SaveHistoryBatch");
            _logger.LogSaveHistoryStoreError(ex, string.Join(",", traces.Select(t => t.Id.ToString())));
            return VKResult.Failure(VKPersistenceErrors.Database.ExecutionFailed);
        }
    }
}
